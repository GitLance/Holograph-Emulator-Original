Imports System
Imports System.Threading
Imports System.Net.Sockets
Imports System.Text
Imports System.Collections
Public Class clsHoloUSER
#Region "User server class properties"
    '// Socket
    Private userSocket As Socket '// Users socket instance
    Friend classID As Integer '// Users socket/class ID
    Friend UserID As Integer '// Users ID in database
    Friend userDetails As clsHoloUSERDETAILS '// Users set of data

    Private byteDataGroup(1024) As Byte '// User's current incoming data group
    Private currentPacket As String '// Current processing packet, so the subs can access it
    Private killedConnection As Boolean '// To prevent multi-use of the killConnection void
    Private timeOut As Byte '// Users connection status
    Private pingManager As Thread
    Private roomCommunicator As clsHoloROOM '// Reference to the room class used to communicate between this user and it's room
    Private receivedItemIndex As Boolean '// If the user received that very big Dg packet containing the hof furni folders of all cct's
    Private curHandPage As Integer

    Private Delegate Sub actionThread(ByVal actionKey As String, ByVal actionValue As String, ByVal actionLength As Integer) '// A delegate this class can use
    Private Delegate Sub carryingThread(ByVal itemToCarry As String) '// A delegate for carrying items
    Private Delegate Sub squareHopperThread()
#End Region
#Region "HoloUSER actions for this class"
    Public Sub New(ByVal myClassID As Integer, ByVal myHoloSocket As Socket) '// Create a new instance of HoloUSER and accept connection handling (@@ packet etc)
        Try
            userSocket = myHoloSocket '// Setup the socket for this class
            classID = myClassID '// Set the used socket ID for this class
            Console.WriteLine("[SCKMGR] Established new connection with " & (userSocket.RemoteEndPoint.ToString).Split(":")(0) & " for socket [" & classID & "]")
            transData("@@" & sysChar(1)) '// Send the 'connection accepted' packet

            listenDataArrivals() '// Start listening for incoming data from this user

            pingManager = New Thread(AddressOf pingUser)
            pingManager.Start() '// Start the timeout checker thread

            userDetails = New clsHoloUSERDETAILS(Me) '// Create new instance of clsHoloUSERDETAILS
        Catch
            killConnection("Error at establishing new socket [" & classID & "]")

        End Try
    End Sub
#Region "Socket management"
    Private Sub listenDataArrivals()
        Try
            userSocket.BeginReceive(byteDataGroup, 0, byteDataGroup.Length, SocketFlags.None, New AsyncCallback(AddressOf dataArrived), 0) '// Set a pending data listener, who hits back to dataArrivalCallback when data is arrived =]
        Catch
            killConnection("Disconnected!")
        End Try
    End Sub
    Private Sub dataArrived(ByVal arrivalCallback As IAsyncResult)
        Dim bytesReceived, cPL As Integer
        Dim dataPack As String

        Try
            bytesReceived = userSocket.EndReceive(arrivalCallback) '// Stop the callback and get the bytes received
        Catch '// Disconnected during data arrival
            killConnection("Disconnected!")
            Return
        End Try

        dataPack = filterPacket(Encoding.ASCII.GetString(byteDataGroup, 0, bytesReceived)) '// Convert the byte group to a string

        If dataPack.Length = 0 Then killConnection("Disconnected!") : Return
        If Not (dataPack.Substring(0, 1) = "@") Then killConnection("Invalid packet, @ missing!") : Return '// If the first char of the packet isn't a @ char, then it's invalid packet so drop the connection

        timeOut = 0 '// Everything is okay, packet received, so the user won't be seen as 'timed out'

        '// Packets are sticked to each other in some cases, decoding the header of them will give us the lenghts of the packets inside
        Do Until dataPack.Length = 0
            cPL = HoloENCODING.decodeB64(dataPack.Substring(1, 2))
            currentPacket = dataPack.Substring(3, cPL) : handleCurrentPacket()
            dataPack = dataPack.Substring(cPL + 3)
        Loop

        currentPacket = Nothing '// Release the space used for currentPacket string
        listenDataArrivals() '// Listen for new data-arrivals
    End Sub
    Friend Sub transData(ByVal strData As String)
        Try
            Console.WriteLine(">> " & strData.Replace(sysChar(13), "$13"))
            Dim dataByteGroup() As Byte = Encoding.ASCII.GetBytes(strData) '// Encode the strData to a byte group
            userSocket.BeginSend(dataByteGroup, 0, dataByteGroup.Length, SocketFlags.None, New AsyncCallback(AddressOf transDataComplete), 0)

        Catch
            killConnection("Error at data transfer attempt!") '// Error, kill connection

        End Try
    End Sub
    Private Sub transDataComplete(ByVal transDataCompletedCallback As IAsyncResult)
        Try
            userSocket.EndSend(transDataCompletedCallback) '// Complete the data sending action

        Catch  '// Error, kill connection
            killConnection("Error at data transfer complete!")

        End Try
    End Sub
#End Region
    Private Sub pingUser()
        While True
            Threading.Thread.Sleep(60000) '// Wait 60 seconds
            If timeOut = 2 Then '// If the user hasn't send a packet the last two minutes (so it's timeOut has been updated two times (60 seconds * 2 = 2 minutes) and the user hasn't send a packet)
                killConnection("Timeout, no packet received last 2 minutes.") '// Drop this connection
            Else
                timeOut += 1 '// User still active, just set it's timeOut to +1, it'll be set to 0 when new packet comes so don't worry
                transData("@r" & sysChar(1)) '// Send a 'ping', forcing the client to reply something back
            End If
        End While
    End Sub
    Private Sub errorUser()
        Dim messageID As Integer
        If Not (currentPacket = vbNullString) Then messageID = HoloENCODING.decodeB64(currentPacket.Substring(0, 2))
        transData("DkI" & HoloENCODING.encodeVL64(messageID) & DateTime.Now.ToString & sysChar(2) & sysChar(1))
    End Sub
    Private Sub handleCurrentPacket()
        'Try

        Console.WriteLine("[" & classID & "]  " & currentPacket.Replace(sysChar(13), "{13}"))
        Select Case currentPacket.Substring(0, 2)

            '// Connection
            Case "CN" '// Encryption status
                transData("DUIH" & sysChar(1))

            Case "CJ" '// Formats for the client/RC4 encryption key (disabled)
                transData("DAQBHHIIKHJIPAHQAdd-MM-yyyySAHPBhttp://holographemulator.comQBH")

            Case "CL" '// Login - check ticket + bans, and get user details
                Dim ssoTicket As String = currentPacket.Substring(4) '// Get the SSO login ticket from the packet
                Dim socketIP As String = userSocket.RemoteEndPoint.ToString.Split(":")(0) '// Retrieve this socket's IP address

                Dim userID As Integer = HoloDB.runRead("SELECT id FROM users WHERE ticket_sso = '" & ssoTicket & "' AND ipaddress_last = '" & socketIP & "' LIMIT 1")
                If userID = 0 Then killConnection("User for SSO ticket [" & ssoTicket & "] and IP address [" & socketIP & "] not found!") : Return '// This user hasn't signed in via HoloCMS, there is no user with a pending SSO ticket like this, or the user who logged in via this user on HoloCMS doesn't have the same IP as the one actually using the ticket now, maybe SSO login bruter? Disconnect them and stop here

                Dim banDetails() As String = HoloDB.runReadArray("SELECT ipaddress,date_expire,descr FROM users_bans WHERE userid = '" & userID & "' OR ipaddress = '" & socketIP & "' LIMIT 1") '// Try getting details about "this user/ipaddress is banned"
                If banDetails.Count > 0 Then '// Banned data found for this user/IP address, user/IP address banned
                    If DateTime.Compare(DateTime.Parse(banDetails(0)), DateTime.Now) > 0 Then '// Ban is still active
                        handleBan(banDetails(2)) '// Handle this ban
                        Return
                    Else
                        If banDetails(0) = vbNullString Then '// The IP address field was empty, user WAS just user banned, lift users ban now
                            HoloDB.runQuery("DELETE FROM users_bans WHERE userid = '" & userID & "' LIMIT 1")
                        Else '// This whole IP address was banned, lift ban now
                            HoloDB.runQuery("DELETE FROM users_bans WHERE ipaddress = '" & socketIP & "' LIMIT 1")
                        End If
                    End If
                Else '// Not banned, proceed login
                    '// Inherited from old V9 login retros, drop already logged instances of this user
                    Dim oldSessionUser As clsHoloUSER = HoloMANAGERS.getUserClass(userID)
                    If IsNothing(oldSessionUser) = False Then '// Still clone active
                        oldSessionUser.killConnection("New instance of this user logged in.")
                        If HoloMANAGERS.hookedUsers.ContainsKey(userID) = True Then HoloMANAGERS.hookedUsers.Remove(userID) '// For some reason kill connection didn't removed the old user from the hooked users hashtable
                    Else
                        Me.UserID = userID
                        userDetails = New clsHoloUSERDETAILS(Me)
                        Dim userData() As String = HoloDB.runReadArray("SELECT name,figure,sex,mission,rank,consolemission FROM users WHERE id = '" & userID & "' LIMIT 1") '// Get users details
                        userDetails.UserID = userID
                        userDetails.Name = userData(0)
                        userDetails.Figure = userData(1)
                        userDetails.Sex = Char.Parse(userData(2))
                        userDetails.Mission = userData(3)
                        userDetails.Rank = userData(4)
                        userDetails.consoleMission = userData(5)

                        HoloMANAGERS.hookedUsers.Add(userID, Me)
                        transData("@C" & sysChar(1)) '// Let client proceed with login

                        '// Background shizzle
                        HoloDB.runQuery("UPDATE users SET ticket_sso = NULL WHERE id = '" & userID & "' LIMIT 1") '// Null the users SSO ticket, since it has been used
                        Console.WriteLine("User [" & userDetails.Name & "] authenticated successfully during login! Hey guess what, he loves Nillus! :D")
                    End If
                End If

            Case "@G" '// Send users appearance etc + the fuserights
                transData("@B" & HoloRANK(userDetails.Rank).strFuse & sysChar(1)) '// Send users fuserights matching his rank
                refreshAppearance(False)

                '// If welcome message enabled, then send it
                If Not (HoloRACK.welcMessage = vbNullString) Then transData("BK" & HoloRACK.welcMessage.Replace("%name%", userDetails.Name).Replace("%release%", My.Application.Info.Version.ToString) & sysChar(1))

            Case "@H" '// Send users valueables (credits, tickets blablah) and the welcom message)
                refreshValuables()

            Case "B]" '// Get user's badges
                processBadges()

            Case "@L" '// Process user's console
                processConsole()

            Case "@Z" '// Process user's Club subscription stats + badges
                processClub()

            Case "@O" '// Update the 'lastvisit' field for user's console
                Console_UpdateStats()
                HoloDB.runQuery("UPDATE users SET lastvisit = '" & DateTime.Now.ToString & "' WHERE id = '" & UserID & "' LIMIT 1")

            Case "@d" '// Change user console mission
                Dim consoleMission As String = HoloMISC.filterWord(currentPacket.Substring(4).Trim, userDetails.Rank)
                transData("BS" & consoleMission & sysChar(2) & sysChar(1))
                HoloDB.runQuery("UPDATE users SET consolemission = '" & HoloDB.fixChars(consoleMission) & "' WHERE id = '" & UserID & "' LIMIT 1")
                userDetails.consoleMission = consoleMission

            Case "@i" '// User performs a user search at Console
                Dim searchInput As String = currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2)))
                Dim searchResult As String() = HoloDB.runReadArray("SELECT id,name,figure,consolemission,lastvisit FROM users WHERE name = '" & searchInput & "' LIMIT 1")
                If searchResult.Count > 0 Then '// There was a user matching this name found!
                    transData("B@MESSENGER" & sysChar(2) & HoloENCODING.encodeVL64(searchResult(0)) & searchResult(1) & sysChar(2) & "I" & searchResult(3) & sysChar(2) & HoloMANAGERS.getUserHotelPosition(Integer.Parse(searchResult(0))) & sysChar(2) & searchResult(4) & sysChar(2) & searchResult(2) & sysChar(2) & sysChar(1))
                Else
                    transData("B@MESSENGER" & sysChar(2) & "H" & sysChar(1))
                End If

            Case "@g" '// User asks someone as friend at Console
                Console_FriendRequest()

            Case "@e" '// User accepts a friendrequest on the Console
                Console_FriendAccept()

            Case "@f" '// User declines a friendrequest on the Console
                Console_FriendDecline()

            Case "@h" '// User deletes (a) friend(s) on the Console
                Console_FriendDelete()

            Case "@a" '// User sends (a) message(s) on the Console
                Console_SendMessage()

            Case "@`" '// User deletes (a) message(s) on the Console
                Console_DeleteMessage()

            Case "DF" '// User 'stalks' a friend 
                Console_FriendFollow()

            Case "BV" '// User performs something on the Navigator (browsing etc)
                handleNavigatorAction()

            Case "BW" '// Guestroom category index for create/modify room
                Dim stagePack As New StringBuilder("C]")
                Dim catStages() As String = HoloDB.runReadArray("SELECT id,name FROM nav_categories WHERE ispubcat = '0' AND parent > '0' AND minrank <= '" & userDetails.Rank & "' ORDER BY id ASC", True)
                stagePack.Append(HoloENCODING.encodeVL64(catStages.Count))

                For i = 0 To catStages.Count - 1
                    Dim stageData() As String = catStages(i).Split(sysChar(9))
                    stagePack.Append(HoloENCODING.encodeVL64(stageData(0)) & stageData(1) & sysChar(2))
                Next

                stagePack.Append(sysChar(1))
                transData(stagePack.ToString)

            Case "BZ" '// Hotel Navigator - Publicroom - who's in here?
                Try
                    transData("C_" & DirectCast(HoloMANAGERS.hookedRooms(HoloENCODING.decodeVL64(currentPacket.Substring(2))), clsHoloROOM).whosInHereList & sysChar(1))
                Catch
                    transData("C_" & sysChar(1))
                End Try

            Case "DH" '// User refreshes recommended rooms section
                handleNavigatorAction_RecRooms()

            Case "BA" '// User enters a voucher
                checkVoucher()

            Case "@Q" '// User searches a guestroom
                searchRoom()

            Case "@P" '// User views his/her own rooms
                seeMyRooms()

            Case "@R" '// User initializes his/her favourite rooms
                Favourites_Init()

            Case "@S" '// User adds a room to his/her favourite rooms
                Favourites_AddRoom()

            Case "@T" '// User removes a room from his/her favourite rooms
                Favourites_DeleteRoom()

            Case "@]" '// Create guestroom - phase 1
                roomModifier(1)

            Case "@Y" '// Modify guestroom / create guestroom - phase 2
                roomModifier(2)

            Case "@U" '// Check guestroom in Navigator (send @v packet)
                GuestRoom_CheckID()

            Case "BX" '// Modify guestroom - click button, send category
                roomModifier(3)

            Case "@X" '// '// Modify guestroom - save name, state and show/hide ownername
                roomModifier(4)

            Case "@W" '// Modify guestroom - delete room
                roomModifier(5)

            Case "B[" '// Modify guestroom - reset all rights in room
                roomModifier(6)

            Case "@u" '// Go to Hotel View (kick!)
                Room_noRoom(True, True)

            Case "Bv" '// Enter room - loading screen advertisement
                Dim roomAdvertisement As String = "http://ads.habbohotel.co.uk/max/adview.php?zoneid=325&n=hhuk	http://ads.habbohotel.co.uk/max/adclick.php?n=hhuk"
                roomAdvertisement = vbNullString
                If roomAdvertisement = vbNullString Then
                    transData("DB" & "0" & sysChar(1))
                Else
                    transData("DB" & roomAdvertisement & sysChar(1))
                End If

            Case "@B" '// Enter room - determine ID and cct name
                Dim isPub As Boolean = currentPacket.Substring(2, 1) = "A" '// Guestroom or publicroom?
                Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get room ID

                If userDetails.roomID > 0 Then Room_noRoom(True, False)
                userDetails.roomID = roomID
                userDetails.inPublicroom = isPub

                transData("@S" & sysChar(1) & "Bf" & "http://www.holographemulator.com/" & sysChar(1))

                Dim aeName As String
                If isPub = True Then aeName = HoloDB.runRead("SELECT name_ae FROM publicrooms WHERE id = '" & roomID & "' LIMIT 1") Else aeName = "model_" & HoloMISC.getRoomModelChar(HoloDB.runRead("SELECT model FROM guestrooms WHERE id = '" & roomID & "' LIMIT 1"))

                transData("AE" & aeName & " " & roomID & sysChar(1))

            Case "@y" '// Enter guestroom - determine state (closed, doorbell etc)end 
                If userDetails.roomID > 0 And userDetails.inPublicroom = False Then GuestRoom_CheckState()

            Case "Ab" '// Guestroom - answer the doorbell
                If userDetails.hasRights = False Then Return '// User doesn't has the right to answer doorbells

                Dim ringingOne As String = currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2)))
                Dim letIn As Boolean = currentPacket.Substring(currentPacket.Length - 1) = "A"

                Dim ringingUser As clsHoloUSERDETAILS = HoloMANAGERS.getUserDetails(Integer.Parse(HoloDB.runRead("SELECT id FROM users WHERE name = '" & ringingOne & "' LIMIT 1")))
                If IsNothing(ringingUser) Then Return '// The doorbelling one has gone offline, stop here

                If letIn = True Then
                    ringingUser.isAllowedInRoom = True
                    ringingUser.userClass.transData("@i" & sysChar(1))
                Else
                    ringingUser.userClass.transData("BC" & sysChar(1))
                End If

            Case "A~" '// Enter room - room advertisement (publicrooms, CP0 = no advertisement)
                transData("CP0" & sysChar(1))

            Case "@|" '// Enter room - hook up with class, get heightmap + room AE incase it's a guestroom
                If HoloMANAGERS.hookedRooms.ContainsKey(userDetails.roomID) = True Then
                    roomCommunicator = HoloMANAGERS.hookedRooms(userDetails.roomID)
                Else
                    Dim newRoom As New clsHoloROOM(userDetails.roomID, userDetails.inPublicroom) '// Setup new room!
                    HoloMANAGERS.hookedRooms.Add(userDetails.roomID, newRoom)
                    roomCommunicator = newRoom
                End If
                transData("@_" & roomCommunicator.Heightmap & sysChar(1))

            Case "@d{" '// Sprites! =x
                If receivedItemIndex = False Then
                    transData("Dg[_Dshelves_norjaX~Dshelves_polyfonYmAshelves_siloXQHtable_polyfon_smallYmAchair_polyfonZbBtable_norja_medY_Itable_silo_medX~Dtable_plasto_4legY_Itable_plasto_roundY_Itable_plasto_bigsquareY_Istand_polyfon_zZbBchair_siloX~Dsofa_siloX~Dcouch_norjaX~Dchair_norjaX~Dtable_polyfon_medYmAdoormat_loveZbBdoormat_plainZ[Msofachair_polyfonX~Dsofa_polyfonZ[Msofachair_siloX~Dchair_plastyX~Dchair_plastoYmAtable_plasto_squareY_Ibed_polyfonX~Dbed_polyfon_one[dObed_trad_oneYmAbed_tradYmAbed_silo_oneYmAbed_silo_twoYmAtable_silo_smallX~Dbed_armas_twoYmAbed_budget_oneXQHbed_budgetXQHshelves_armasYmAbench_armasYmAtable_armasYmAsmall_table_armasZbBsmall_chair_armasYmAfireplace_armasYmAlamp_armasYmAbed_armas_oneYmAcarpet_standardY_Icarpet_armasYmAcarpet_polarY_Ifireplace_polyfonY_Itable_plasto_4leg*1Y_Itable_plasto_bigsquare*1Y_Itable_plasto_round*1Y_Itable_plasto_square*1Y_Ichair_plasto*1YmAcarpet_standard*1Y_Idoormat_plain*1Z[Mtable_plasto_4leg*2Y_Itable_plasto_bigsquare*2Y_Itable_plasto_round*2Y_Itable_plasto_square*2Y_Ichair_plasto*2YmAdoormat_plain*2Z[Mcarpet_standard*2Y_Itable_plasto_4leg*3Y_Itable_plasto_bigsquare*3Y_Itable_plasto_round*3Y_Itable_plasto_square*3Y_Ichair_plasto*3YmAcarpet_standard*3Y_Idoormat_plain*3Z[Mtable_plasto_4leg*4Y_Itable_plasto_bigsquare*4Y_Itable_plasto_round*4Y_Itable_plasto_square*4Y_Ichair_plasto*4YmAcarpet_standard*4Y_Idoormat_plain*4Z[Mdoormat_plain*6Z[Mdoormat_plain*5Z[Mcarpet_standard*5Y_Itable_plasto_4leg*5Y_Itable_plasto_bigsquare*5Y_Itable_plasto_round*5Y_Itable_plasto_square*5Y_Ichair_plasto*5YmAtable_plasto_4leg*6Y_Itable_plasto_bigsquare*6Y_Itable_plasto_round*6Y_Itable_plasto_square*6Y_Ichair_plasto*6YmAtable_plasto_4leg*7Y_Itable_plasto_bigsquare*7Y_Itable_plasto_round*7Y_Itable_plasto_square*7Y_Ichair_plasto*7YmAtable_plasto_4leg*8Y_Itable_plasto_bigsquare*8Y_Itable_plasto_round*8Y_Itable_plasto_square*8Y_Ichair_plasto*8YmAtable_plasto_4leg*9Y_Itable_plasto_bigsquare*9Y_Itable_plasto_round*9Y_Itable_plasto_square*9Y_Ichair_plasto*9YmAcarpet_standard*6Y_Ichair_plasty*1X~DpizzaYmAdrinksYmAchair_plasty*2X~Dchair_plasty*3X~Dchair_plasty*4X~Dbar_polyfonY_Iplant_cruddyYmAbottleYmAbardesk_polyfonX~Dbardeskcorner_polyfonX~DfloortileHbar_armasY_Ibartable_armasYmAbar_chair_armasYmAcarpet_softZ@Kcarpet_soft*1Z@Kcarpet_soft*2Z@Kcarpet_soft*3Z@Kcarpet_soft*4Z@Kcarpet_soft*5Z@Kcarpet_soft*6Z@Kred_tvY_Iwood_tvYmAcarpet_polar*1Y_Ichair_plasty*5X~Dcarpet_polar*2Y_Icarpet_polar*3Y_Icarpet_polar*4Y_Ichair_plasty*6X~Dtable_polyfonYmAsmooth_table_polyfonYmAsofachair_polyfon_girlX~Dbed_polyfon_girl_one[dObed_polyfon_girlX~Dsofa_polyfon_girlZ[Mbed_budgetb_oneXQHbed_budgetbXQHplant_pineappleYmAplant_fruittreeY_Iplant_small_cactusY_Iplant_bonsaiY_Iplant_big_cactusY_Iplant_yukkaY_Icarpet_standard*7Y_Icarpet_standard*8Y_Icarpet_standard*9Y_Icarpet_standard*aY_Icarpet_standard*bY_Iplant_sunflowerY_Iplant_roseY_Itv_luxusY_IbathZ\BsinkY_ItoiletYmAduckYmAtileYmAtoilet_redYmAtoilet_yellYmAtile_redYmAtile_yellYmApresent_gen[~Npresent_gen1[~Npresent_gen2[~Npresent_gen3[~Npresent_gen4[~Npresent_gen5[~Npresent_gen6[~Nbar_basicY_Ishelves_basicXQHsoft_sofachair_norjaX~Dsoft_sofa_norjaX~Dlamp_basicXQHlamp2_armasYmAfridgeY_Idoor[dOdoorB[dOdoorC[dOpumpkinYmAskullcandleYmAdeadduckYmAdeadduck2YmAdeadduck3YmAmenorahYmApuddingYmAhamYmAturkeyYmAxmasduckY_IhouseYmAtriplecandleYmAtree3YmAtree4YmAtree5X~Dham2YmAwcandlesetYmArcandlesetYmAstatueYmAheartY_IvaleduckYmAheartsofaX~DthroneYmAsamovarY_IgiftflowersY_IhabbocakeYmAhologramYmAeasterduckY_IbunnyYmAbasketY_IbirdieYmAediceX~Dclub_sofaZ[Mprize1YmAprize2YmAprize3YmAdivider_poly3X~Ddivider_arm1YmAdivider_arm2YmAdivider_arm3YmAdivider_nor1X~Ddivider_silo1X~Ddivider_nor2X~Ddivider_silo2Z[Mdivider_nor3X~Ddivider_silo3X~DtypingmachineYmAspyroYmAredhologramYmAcameraHjoulutahtiYmAhyacinth1YmAhyacinth2YmAchair_plasto*10YmAchair_plasto*11YmAbardeskcorner_polyfon*12X~Dbardeskcorner_polyfon*13X~Dchair_plasto*12YmAchair_plasto*13YmAchair_plasto*14YmAtable_plasto_4leg*14Y_ImocchamasterY_Icarpet_legocourtYmAbench_legoYmAlegotrophyYmAvalentinescreenYmAedicehcYmArare_daffodil_rugYmArare_beehive_bulbY_IhcsohvaYmAhcammeYmArare_elephant_statueYmArare_fountainY_Irare_standYmArare_globeYmArare_hammockYmArare_elephant_statue*1YmArare_elephant_statue*2YmArare_fountain*1Y_Irare_fountain*2Y_Irare_fountain*3Y_Irare_beehive_bulb*1Y_Irare_beehive_bulb*2Y_Irare_xmas_screenY_Irare_parasol*1Y_Irare_parasol*2Y_Irare_parasol*3Y_Itree1X~Dtree2ZmBwcandleYxBrcandleYxBsoft_jaggara_norjaYmAhouse2YmAdjesko_turntableYmAmd_sofaZ[Mmd_limukaappiY_Itable_plasto_4leg*10Y_Itable_plasto_4leg*15Y_Itable_plasto_bigsquare*14Y_Itable_plasto_bigsquare*15Y_Itable_plasto_round*14Y_Itable_plasto_round*15Y_Itable_plasto_square*14Y_Itable_plasto_square*15Y_Ichair_plasto*15YmAchair_plasty*7X~Dchair_plasty*8X~Dchair_plasty*9X~Dchair_plasty*10X~Dchair_plasty*11X~Dchair_plasto*16YmAtable_plasto_4leg*16Y_Ihockey_scoreY_Ihockey_lightYmAdoorD[dOprizetrophy2*3[rIprizetrophy3*3XrIprizetrophy4*3[rIprizetrophy5*3[rIprizetrophy6*3[rIprizetrophy*1Y_Iprizetrophy2*1[rIprizetrophy3*1XrIprizetrophy4*1[rIprizetrophy5*1[rIprizetrophy6*1[rIprizetrophy*2Y_Iprizetrophy2*2[rIprizetrophy3*2XrIprizetrophy4*2[rIprizetrophy5*2[rIprizetrophy6*2[rIprizetrophy*3Y_Irare_parasol*0Hhc_lmp[fBhc_tblYmAhc_chrYmAhc_dskXQHnestHpetfood1ZvCpetfood2ZvCpetfood3ZvCwaterbowl*4XICwaterbowl*5XICwaterbowl*2XICwaterbowl*1XICwaterbowl*3XICtoy1XICtoy1*1XICtoy1*2XICtoy1*3XICtoy1*4XICgoodie1ZvCgoodie1*1ZvCgoodie1*2ZvCgoodie2X~Dprizetrophy7*3[rIprizetrophy7*1[rIprizetrophy7*2[rIscifiport*0Y_Iscifiport*9Y_Iscifiport*8Y_Iscifiport*7Y_Iscifiport*6Y_Iscifiport*5Y_Iscifiport*4Y_Iscifiport*3Y_Iscifiport*2Y_Iscifiport*1Y_Iscifirocket*9Y_Iscifirocket*8Y_Iscifirocket*7Y_Iscifirocket*6Y_Iscifirocket*5Y_Iscifirocket*4Y_Iscifirocket*3Y_Iscifirocket*2Y_Iscifirocket*1Y_Iscifirocket*0Y_Iscifidoor*10Y_Iscifidoor*9Y_Iscifidoor*8Y_Iscifidoor*7Y_Iscifidoor*6Y_Iscifidoor*5Y_Iscifidoor*4Y_Iscifidoor*3Y_Iscifidoor*2Y_Iscifidoor*1Y_Ipillow*5YmApillow*8YmApillow*0YmApillow*1YmApillow*2YmApillow*7YmApillow*9YmApillow*4YmApillow*6YmApillow*3YmAmarquee*1Y_Imarquee*2Y_Imarquee*7Y_Imarquee*aY_Imarquee*8Y_Imarquee*9Y_Imarquee*5Y_Imarquee*4Y_Imarquee*6Y_Imarquee*3Y_Iwooden_screen*1Y_Iwooden_screen*2Y_Iwooden_screen*7Y_Iwooden_screen*0Y_Iwooden_screen*8Y_Iwooden_screen*5Y_Iwooden_screen*9Y_Iwooden_screen*4Y_Iwooden_screen*6Y_Iwooden_screen*3Y_Ipillar*6Y_Ipillar*1Y_Ipillar*9Y_Ipillar*0Y_Ipillar*8Y_Ipillar*2Y_Ipillar*5Y_Ipillar*4Y_Ipillar*7Y_Ipillar*3Y_Irare_dragonlamp*4Y_Irare_dragonlamp*0Y_Irare_dragonlamp*5Y_Irare_dragonlamp*2Y_Irare_dragonlamp*8Y_Irare_dragonlamp*9Y_Irare_dragonlamp*7Y_Irare_dragonlamp*6Y_Irare_dragonlamp*1Y_Irare_dragonlamp*3Y_Irare_icecream*1Y_Irare_icecream*7Y_Irare_icecream*8Y_Irare_icecream*2Y_Irare_icecream*6Y_Irare_icecream*9Y_Irare_icecream*3Y_Irare_icecream*0Y_Irare_icecream*4Y_Irare_icecream*5Y_Irare_fan*7YxBrare_fan*6YxBrare_fan*9YxBrare_fan*3YxBrare_fan*0YxBrare_fan*4YxBrare_fan*5YxBrare_fan*1YxBrare_fan*8YxBrare_fan*2YxBqueue_tile1*3X~Dqueue_tile1*6X~Dqueue_tile1*4X~Dqueue_tile1*9X~Dqueue_tile1*8X~Dqueue_tile1*5X~Dqueue_tile1*7X~Dqueue_tile1*2X~Dqueue_tile1*1X~Dqueue_tile1*0X~DticketHrare_snowrugX~Dcn_lampZxIcn_sofaYmAsporttrack1*1YmAsporttrack1*3YmAsporttrack1*2YmAsporttrack2*1[~Nsporttrack2*2[~Nsporttrack2*3[~Nsporttrack3*1YmAsporttrack3*2YmAsporttrack3*3YmAfootylampX~Dbarchair_siloX~Ddivider_nor4*4X~Dtraffic_light*1ZxItraffic_light*2ZxItraffic_light*3ZxItraffic_light*4ZxItraffic_light*6ZxIrubberchair*1X~Drubberchair*2X~Drubberchair*3X~Drubberchair*4X~Drubberchair*5X~Drubberchair*6X~Dbarrier*1X~Dbarrier*2X~Dbarrier*3X~Drubberchair*7X~Drubberchair*8X~Dtable_norja_med*2Y_Itable_norja_med*3Y_Itable_norja_med*4Y_Itable_norja_med*5Y_Itable_norja_med*6Y_Itable_norja_med*7Y_Itable_norja_med*8Y_Itable_norja_med*9Y_Icouch_norja*2X~Dcouch_norja*3X~Dcouch_norja*4X~Dcouch_norja*5X~Dcouch_norja*6X~Dcouch_norja*7X~Dcouch_norja*8X~Dcouch_norja*9X~Dshelves_norja*2X~Dshelves_norja*3X~Dshelves_norja*4X~Dshelves_norja*5X~Dshelves_norja*6X~Dshelves_norja*7X~Dshelves_norja*8X~Dshelves_norja*9X~Dchair_norja*2X~Dchair_norja*3X~Dchair_norja*4X~Dchair_norja*5X~Dchair_norja*6X~Dchair_norja*7X~Dchair_norja*8X~Dchair_norja*9X~Ddivider_nor1*2X~Ddivider_nor1*3X~Ddivider_nor1*4X~Ddivider_nor1*5X~Ddivider_nor1*6X~Ddivider_nor1*7X~Ddivider_nor1*8X~Ddivider_nor1*9X~Dsoft_sofa_norja*2X~Dsoft_sofa_norja*3X~Dsoft_sofa_norja*4X~Dsoft_sofa_norja*5X~Dsoft_sofa_norja*6X~Dsoft_sofa_norja*7X~Dsoft_sofa_norja*8X~Dsoft_sofa_norja*9X~Dsoft_sofachair_norja*2X~Dsoft_sofachair_norja*3X~Dsoft_sofachair_norja*4X~Dsoft_sofachair_norja*5X~Dsoft_sofachair_norja*6X~Dsoft_sofachair_norja*7X~Dsoft_sofachair_norja*8X~Dsoft_sofachair_norja*9X~Dsofachair_silo*2X~Dsofachair_silo*3X~Dsofachair_silo*4X~Dsofachair_silo*5X~Dsofachair_silo*6X~Dsofachair_silo*7X~Dsofachair_silo*8X~Dsofachair_silo*9X~Dtable_silo_small*2X~Dtable_silo_small*3X~Dtable_silo_small*4X~Dtable_silo_small*5X~Dtable_silo_small*6X~Dtable_silo_small*7X~Dtable_silo_small*8X~Dtable_silo_small*9X~Ddivider_silo1*2X~Ddivider_silo1*3X~Ddivider_silo1*4X~Ddivider_silo1*5X~Ddivider_silo1*6X~Ddivider_silo1*7X~Ddivider_silo1*8X~Ddivider_silo1*9X~Ddivider_silo3*2X~Ddivider_silo3*3X~Ddivider_silo3*4X~Ddivider_silo3*5X~Ddivider_silo3*6X~Ddivider_silo3*7X~Ddivider_silo3*8X~Ddivider_silo3*9X~Dtable_silo_med*2X~Dtable_silo_med*3X~Dtable_silo_med*4X~Dtable_silo_med*5X~Dtable_silo_med*6X~Dtable_silo_med*7X~Dtable_silo_med*8X~Dtable_silo_med*9X~Dsofa_silo*2X~Dsofa_silo*3X~Dsofa_silo*4X~Dsofa_silo*5X~Dsofa_silo*6X~Dsofa_silo*7X~Dsofa_silo*8X~Dsofa_silo*9X~Dsofachair_polyfon*2X~Dsofachair_polyfon*3X~Dsofachair_polyfon*4X~Dsofachair_polyfon*6X~Dsofachair_polyfon*7X~Dsofachair_polyfon*8X~Dsofachair_polyfon*9X~Dsofa_polyfon*2Z[Msofa_polyfon*3Z[Msofa_polyfon*4Z[Msofa_polyfon*6Z[Msofa_polyfon*7Z[Msofa_polyfon*8Z[Msofa_polyfon*9Z[Mbed_polyfon*2X~Dbed_polyfon*3X~Dbed_polyfon*4X~Dbed_polyfon*6X~Dbed_polyfon*7X~Dbed_polyfon*8X~Dbed_polyfon*9X~Dbed_polyfon_one*2[dObed_polyfon_one*3[dObed_polyfon_one*4[dObed_polyfon_one*6[dObed_polyfon_one*7[dObed_polyfon_one*8[dObed_polyfon_one*9[dObardesk_polyfon*2X~Dbardesk_polyfon*3X~Dbardesk_polyfon*4X~Dbardesk_polyfon*5X~Dbardesk_polyfon*6X~Dbardesk_polyfon*7X~Dbardesk_polyfon*8X~Dbardesk_polyfon*9X~Dbardeskcorner_polyfon*2X~Dbardeskcorner_polyfon*3X~Dbardeskcorner_polyfon*4X~Dbardeskcorner_polyfon*5X~Dbardeskcorner_polyfon*6X~Dbardeskcorner_polyfon*7X~Dbardeskcorner_polyfon*8X~Dbardeskcorner_polyfon*9X~Ddivider_poly3*2X~Ddivider_poly3*3X~Ddivider_poly3*4X~Ddivider_poly3*5X~Ddivider_poly3*6X~Ddivider_poly3*7X~Ddivider_poly3*8X~Ddivider_poly3*9X~Dchair_silo*2X~Dchair_silo*3X~Dchair_silo*4X~Dchair_silo*5X~Dchair_silo*6X~Dchair_silo*7X~Dchair_silo*8X~Dchair_silo*9X~Ddivider_nor3*2X~Ddivider_nor3*3X~Ddivider_nor3*4X~Ddivider_nor3*5X~Ddivider_nor3*6X~Ddivider_nor3*7X~Ddivider_nor3*8X~Ddivider_nor3*9X~Ddivider_nor2*2X~Ddivider_nor2*3X~Ddivider_nor2*4X~Ddivider_nor2*5X~Ddivider_nor2*6X~Ddivider_nor2*7X~Ddivider_nor2*8X~Ddivider_nor2*9X~Dsilo_studydeskX~Dsolarium_norjaY_Isolarium_norja*1Y_Isolarium_norja*2Y_Isolarium_norja*3Y_Isolarium_norja*5Y_Isolarium_norja*6Y_Isolarium_norja*7Y_Isolarium_norja*8Y_Isolarium_norja*9Y_IsandrugX~Drare_moonrugYmAchair_chinaYmAchina_tableYmAsleepingbag*1YmAsleepingbag*2YmAsleepingbag*3YmAsleepingbag*4YmAsafe_siloY_Isleepingbag*7YmAsleepingbag*9YmAsleepingbag*5YmAsleepingbag*10YmAsleepingbag*6YmAsleepingbag*8YmAchina_shelveX~Dtraffic_light*5ZxIdivider_nor4*2X~Ddivider_nor4*3X~Ddivider_nor4*5X~Ddivider_nor4*6X~Ddivider_nor4*7X~Ddivider_nor4*8X~Ddivider_nor4*9X~Ddivider_nor5*2X~Ddivider_nor5*3X~Ddivider_nor5*4X~Ddivider_nor5*5X~Ddivider_nor5*6X~Ddivider_nor5*7X~Ddivider_nor5*8X~Ddivider_nor5*9X~Ddivider_nor5X~Ddivider_nor4X~Dwall_chinaYmAcorner_chinaYmAbarchair_silo*2X~Dbarchair_silo*3X~Dbarchair_silo*4X~Dbarchair_silo*5X~Dbarchair_silo*6X~Dbarchair_silo*7X~Dbarchair_silo*8X~Dbarchair_silo*9X~Dsafe_silo*2Y_Isafe_silo*3Y_Isafe_silo*4Y_Isafe_silo*5Y_Isafe_silo*6Y_Isafe_silo*7Y_Isafe_silo*8Y_Isafe_silo*9Y_Iglass_shelfY_Iglass_chairY_Iglass_stoolY_Iglass_sofaY_Iglass_tableY_Iglass_table*2Y_Iglass_table*3Y_Iglass_table*4Y_Iglass_table*5Y_Iglass_table*6Y_Iglass_table*7Y_Iglass_table*8Y_Iglass_table*9Y_Iglass_chair*2Y_Iglass_chair*3Y_Iglass_chair*4Y_Iglass_chair*5Y_Iglass_chair*6Y_Iglass_chair*7Y_Iglass_chair*8Y_Iglass_chair*9Y_Iglass_sofa*2Y_Iglass_sofa*3Y_Iglass_sofa*4Y_Iglass_sofa*5Y_Iglass_sofa*6Y_Iglass_sofa*7Y_Iglass_sofa*8Y_Iglass_sofa*9Y_Iglass_stool*2Y_Iglass_stool*4Y_Iglass_stool*5Y_Iglass_stool*6Y_Iglass_stool*7Y_Iglass_stool*8Y_Iglass_stool*3Y_Iglass_stool*9Y_ICFC_100_coin_goldZvCCFC_10_coin_bronzeZvCCFC_200_moneybagZvCCFC_500_goldbarZvCCFC_50_coin_silverZvCCF_10_coin_goldZvCCF_1_coin_bronzeZvCCF_20_moneybagZvCCF_50_goldbarZvCCF_5_coin_silverZvChc_crptYmAhc_tvZ\BgothgateX~DgothiccandelabraYxBgothrailingX~Dgoth_tableYmAhc_bkshlfYmAhc_btlrY_Ihc_crtnYmAhc_djsetYmAhc_frplcZbBhc_lmpstYmAhc_machineYmAhc_rllrXQHhc_rntgnX~Dhc_trllYmAgothic_chair*1X~Dgothic_sofa*1X~Dgothic_stool*1X~Dgothic_chair*2X~Dgothic_sofa*2X~Dgothic_stool*2X~Dgothic_chair*3X~Dgothic_sofa*3X~Dgothic_stool*3X~Dgothic_chair*4X~Dgothic_sofa*4X~Dgothic_stool*4X~Dgothic_chair*5X~Dgothic_sofa*5X~Dgothic_stool*5X~Dgothic_chair*6X~Dgothic_sofa*6X~Dgothic_stool*6X~Dval_cauldronX~Dsound_machineX~Dromantique_pianochair*3Y_Iromantique_pianochair*5Y_Iromantique_pianochair*2Y_Iromantique_pianochair*4Y_Iromantique_pianochair*1Y_Iromantique_divan*3Y_Iromantique_divan*5Y_Iromantique_divan*2Y_Iromantique_divan*4Y_Iromantique_divan*1Y_Iromantique_chair*3Y_Iromantique_chair*5Y_Iromantique_chair*2Y_Iromantique_chair*4Y_Iromantique_chair*1Y_Irare_parasolY_Iplant_valentinerose*3XICplant_valentinerose*5XICplant_valentinerose*2XICplant_valentinerose*4XICplant_valentinerose*1XICplant_mazegateYeCplant_mazeZcCplant_bulrushXICpetfood4Y_Icarpet_valentineZ|Egothic_carpetXICgothic_carpet2Z|Egothic_chairX~Dgothic_sofaX~Dgothic_stoolX~Dgrand_piano*3Z|Egrand_piano*5Z|Egrand_piano*2Z|Egrand_piano*4Z|Egrand_piano*1Z|Etheatre_seatZ@Kromantique_tray2Y_Iromantique_tray1Y_Iromantique_smalltabl*3Y_Iromantique_smalltabl*5Y_Iromantique_smalltabl*2Y_Iromantique_smalltabl*4Y_Iromantique_smalltabl*1Y_Iromantique_mirrortablY_Iromantique_divider*3Z[Mromantique_divider*2Z[Mromantique_divider*4Z[Mromantique_divider*1Z[Mjp_tatami2YGGjp_tatamiYGGhabbowood_chairYGGjp_bambooYGGjp_iroriXQHjp_pillowYGGsound_set_1Y_Isound_set_2Y_Isound_set_3Y_Isound_set_4Y_Isound_set_5Z@Ksound_set_6Y_Isound_set_7Y_Isound_set_8Y_Isound_set_9Y_Isound_machine*1ZIPspotlightY_Isound_machine*2ZIPsound_machine*3ZIPsound_machine*4ZIPsound_machine*5ZIPsound_machine*6ZIPsound_machine*7ZIProm_lampZ|Erclr_sofaXQHrclr_gardenXQHrclr_chairZ|Esound_set_28Y_Isound_set_27Y_Isound_set_26Y_Isound_set_25Y_Isound_set_24Y_Isound_set_23Y_Isound_set_22Y_Isound_set_21Y_Isound_set_20Z@Ksound_set_19Z@Ksound_set_18Y_Isound_set_17Y_Isound_set_16Y_Isound_set_15Y_Isound_set_14Y_Isound_set_13Y_Isound_set_12Y_Isound_set_11Y_Isound_set_10Y_Irope_dividerXQHromantique_clockY_Irare_icecream_campaignY_Ipura_mdl5*1XQHpura_mdl5*2XQHpura_mdl5*3XQHpura_mdl5*4XQHpura_mdl5*5XQHpura_mdl5*6XQHpura_mdl5*7XQHpura_mdl5*8XQHpura_mdl5*9XQHpura_mdl4*1XQHpura_mdl4*2XQHpura_mdl4*3XQHpura_mdl4*4XQHpura_mdl4*5XQHpura_mdl4*6XQHpura_mdl4*7XQHpura_mdl4*8XQHpura_mdl4*9XQHpura_mdl3*1XQHpura_mdl3*2XQHpura_mdl3*3XQHpura_mdl3*4XQHpura_mdl3*5XQHpura_mdl3*6XQHpura_mdl3*7XQHpura_mdl3*8XQHpura_mdl3*9XQHpura_mdl2*1XQHpura_mdl2*2XQHpura_mdl2*3XQHpura_mdl2*4XQHpura_mdl2*5XQHpura_mdl2*6XQHpura_mdl2*7XQHpura_mdl2*8XQHpura_mdl2*9XQHpura_mdl1*1XQHpura_mdl1*2XQHpura_mdl1*3XQHpura_mdl1*4XQHpura_mdl1*5XQHpura_mdl1*6XQHpura_mdl1*7XQHpura_mdl1*8XQHpura_mdl1*9XQHjp_lanternXQHchair_basic*1XQHchair_basic*2XQHchair_basic*3XQHchair_basic*4XQHchair_basic*5XQHchair_basic*6XQHchair_basic*7XQHchair_basic*8XQHchair_basic*9XQHbed_budget*1XQHbed_budget*2XQHbed_budget*3XQHbed_budget*4XQHbed_budget*5XQHbed_budget*6XQHbed_budget*7XQHbed_budget*8XQHbed_budget*9XQHbed_budget_one*1XQHbed_budget_one*2XQHbed_budget_one*3XQHbed_budget_one*4XQHbed_budget_one*5XQHbed_budget_one*6XQHbed_budget_one*7XQHbed_budget_one*8XQHbed_budget_one*9XQHjp_drawerXQHtile_stellaZ[Mtile_marbleZ[Mtile_brownZ[Msummer_grill*1Y_Isummer_grill*2Y_Isummer_grill*3Y_Isummer_grill*4Y_Isummer_chair*1Y_Isummer_chair*2Y_Isummer_chair*3Y_Isummer_chair*4Y_Isummer_chair*5Y_Isummer_chair*6Y_Isummer_chair*7Y_Isummer_chair*8Y_Isummer_chair*9Y_Isound_set_36ZfIsound_set_35ZfIsound_set_34ZfIsound_set_33ZfIsound_set_32Y_Isound_set_31Y_Isound_set_30Y_Isound_set_29Y_Isound_machine_pro[~Nrare_mnstrY_Ione_way_door*1XQHone_way_door*2XQHone_way_door*3XQHone_way_door*4XQHone_way_door*5XQHone_way_door*6XQHone_way_door*7XQHone_way_door*8XQHone_way_door*9XQHexe_rugZ[Mexe_s_tableZGRsound_set_37ZfIsummer_pool*1ZlIsummer_pool*2ZlIsummer_pool*3ZlIsummer_pool*4ZlIsong_diskY_Ijukebox*1[~Ncarpet_soft_tut[~Nsound_set_44Z@Ksound_set_43Z@Ksound_set_42Z@Ksound_set_41Z@Ksound_set_40Z@Ksound_set_39Z@Ksound_set_38Z@Kgrunge_chairZ@Kgrunge_mattressZ@Kgrunge_radiatorZ@Kgrunge_shelfZ@Kgrunge_signZ@Kgrunge_tableZ@Khabboween_crypt[uKhabboween_grassZ@Khal_cauldronZ@Khal_graveZ@Ksound_set_52ZuKsound_set_51ZuKsound_set_50ZuKsound_set_49ZuKsound_set_48ZuKsound_set_47ZuKsound_set_46ZuKsound_set_45ZuKxmas_icelampZ[Mxmas_cstl_wallZ[Mxmas_cstl_twrZ[Mxmas_cstl_gate[~Ntree7Z[Mtree6Z[Msound_set_54Z[Msound_set_53Z[Msafe_silo_pb[dOplant_mazegate_snowZ[Mplant_maze_snowZ[Mchristmas_sleighZ[Mchristmas_reindeer[~Nchristmas_poopZ[Mexe_bardeskZ[Mexe_chairZ[Mexe_chair2Z[Mexe_cornerZ[Mexe_drinksZ[Mexe_sofaZ[Mexe_tableZ[Msound_set_59[~Nsound_set_58[~Nsound_set_57[~Nsound_set_56[~Nsound_set_55[~Nnoob_table*1[~Nnoob_table*2[~Nnoob_table*3[~Nnoob_table*4[~Nnoob_table*5[~Nnoob_table*6[~Nnoob_stool*1[~Nnoob_stool*2[~Nnoob_stool*3[~Nnoob_stool*4[~Nnoob_stool*5[~Nnoob_stool*6[~Nnoob_rug*1[~Nnoob_rug*2[~Nnoob_rug*3[~Nnoob_rug*4[~Nnoob_rug*5[~Nnoob_rug*6[~Nnoob_lamp*1[dOnoob_lamp*2[dOnoob_lamp*3[dOnoob_lamp*4[dOnoob_lamp*5[dOnoob_lamp*6[dOnoob_chair*1[~Nnoob_chair*2[~Nnoob_chair*3[~Nnoob_chair*4[~Nnoob_chair*5[~Nnoob_chair*6[~Nexe_globe[~Nexe_plantZ[Mval_teddy*1[dOval_teddy*2[dOval_teddy*3[dOval_teddy*4[dOval_teddy*5[dOval_teddy*6[dOval_randomizer[dOval_choco[dOteleport_door[dOsound_set_61[dOsound_set_60[dOfortune[dOsw_tableZIPsw_raven[cQsw_chestZIPsand_cstl_wallZIPsand_cstl_twrZIPsand_cstl_gateZIPgrunge_candleZIPgrunge_benchZIPgrunge_barrelZIPrclr_lampZGRprizetrophy9*1ZGRprizetrophy8*1ZGRnouvelle_traxYcPmd_rugZGRjp_tray6ZGRjp_tray5ZGRjp_tray4ZGRjp_tray3ZGRjp_tray2ZGRjp_tray1ZGRarabian_teamkZGRarabian_snakeZGRarabian_rugZGRarabian_pllwZGRarabian_divdrZGRarabian_chairZGRarabian_bigtbZGRarabian_tetblZGRarabian_tray1ZGRarabian_tray2ZGRarabian_tray3ZGRarabian_tray4ZGRPIpost.itHpost.it.vdHphotoHChessHTicTacToeHBattleShipHPokerHwallpaperHfloorHposterZ@KgothicfountainYxBhc_wall_lampZbBindustrialfanZ`BtorchZ\Bval_heartXBCwallmirrorZ|Ejp_ninjastarsXQHhabw_mirrorXQHhabbowheelZ[Mguitar_skullZ@Kguitar_vZ@Kxmas_light[~Nhrella_poster_3[Nhrella_poster_2ZIPhrella_poster_1[Nsw_swordsZIPsw_stoneZIPsw_holeZIProomdimmerZGRmd_logo_wallZGRmd_canZGRjp_sheet3ZGRjp_sheet2ZGRjp_sheet1ZGRarabian_swordsZGRarabian_wndwZGR")
                    receivedItemIndex = True
                End If
                transData("DiH" & sysChar(1))

            Case "@}" '// Enter room - get inside users
                transData("@\" & roomCommunicator.insideUsers & sysChar(1))
                If userDetails.inPublicroom = True Then userDetails.isAllowedInRoom = True

            Case "@~" '// Enter room - get furni items, wallitems and other publicroom items
                transData("@`" & roomCommunicator.Items & sysChar(1))
                transData("@^" & roomCommunicator.otherItems & sysChar(1))
                If roomCommunicator.isPublicRoom = False Then
                    Dim roomDecor() As String = HoloDB.runRead("SELECT decoration FROM guestrooms WHERE id = '" & userDetails.roomID & "' LIMIT 1").Split("/")
                    If roomDecor(0) > 0 Then transData("@nwallpaper/" & roomDecor(0) & sysChar(1)) '// If the room has wallpaper, send it
                    If roomDecor(1) > 0 Then transData("@nfloor/" & roomDecor(1) & sysChar(1)) '// If the room has a floor carpet, send it

                    transData("@m" & roomCommunicator.wallItems & sysChar(1))
                    transData("DiH" & sysChar(1))

                    If receivedItemIndex = False Then
                        transData("Dg[_Dshelves_norjaX~Dshelves_polyfonYmAshelves_siloXQHtable_polyfon_smallYmAchair_polyfonZbBtable_norja_medY_Itable_silo_medX~Dtable_plasto_4legY_Itable_plasto_roundY_Itable_plasto_bigsquareY_Istand_polyfon_zZbBchair_siloX~Dsofa_siloX~Dcouch_norjaX~Dchair_norjaX~Dtable_polyfon_medYmAdoormat_loveZbBdoormat_plainZ[Msofachair_polyfonX~Dsofa_polyfonZ[Msofachair_siloX~Dchair_plastyX~Dchair_plastoYmAtable_plasto_squareY_Ibed_polyfonX~Dbed_polyfon_one[dObed_trad_oneYmAbed_tradYmAbed_silo_oneYmAbed_silo_twoYmAtable_silo_smallX~Dbed_armas_twoYmAbed_budget_oneXQHbed_budgetXQHshelves_armasYmAbench_armasYmAtable_armasYmAsmall_table_armasZbBsmall_chair_armasYmAfireplace_armasYmAlamp_armasYmAbed_armas_oneYmAcarpet_standardY_Icarpet_armasYmAcarpet_polarY_Ifireplace_polyfonY_Itable_plasto_4leg*1Y_Itable_plasto_bigsquare*1Y_Itable_plasto_round*1Y_Itable_plasto_square*1Y_Ichair_plasto*1YmAcarpet_standard*1Y_Idoormat_plain*1Z[Mtable_plasto_4leg*2Y_Itable_plasto_bigsquare*2Y_Itable_plasto_round*2Y_Itable_plasto_square*2Y_Ichair_plasto*2YmAdoormat_plain*2Z[Mcarpet_standard*2Y_Itable_plasto_4leg*3Y_Itable_plasto_bigsquare*3Y_Itable_plasto_round*3Y_Itable_plasto_square*3Y_Ichair_plasto*3YmAcarpet_standard*3Y_Idoormat_plain*3Z[Mtable_plasto_4leg*4Y_Itable_plasto_bigsquare*4Y_Itable_plasto_round*4Y_Itable_plasto_square*4Y_Ichair_plasto*4YmAcarpet_standard*4Y_Idoormat_plain*4Z[Mdoormat_plain*6Z[Mdoormat_plain*5Z[Mcarpet_standard*5Y_Itable_plasto_4leg*5Y_Itable_plasto_bigsquare*5Y_Itable_plasto_round*5Y_Itable_plasto_square*5Y_Ichair_plasto*5YmAtable_plasto_4leg*6Y_Itable_plasto_bigsquare*6Y_Itable_plasto_round*6Y_Itable_plasto_square*6Y_Ichair_plasto*6YmAtable_plasto_4leg*7Y_Itable_plasto_bigsquare*7Y_Itable_plasto_round*7Y_Itable_plasto_square*7Y_Ichair_plasto*7YmAtable_plasto_4leg*8Y_Itable_plasto_bigsquare*8Y_Itable_plasto_round*8Y_Itable_plasto_square*8Y_Ichair_plasto*8YmAtable_plasto_4leg*9Y_Itable_plasto_bigsquare*9Y_Itable_plasto_round*9Y_Itable_plasto_square*9Y_Ichair_plasto*9YmAcarpet_standard*6Y_Ichair_plasty*1X~DpizzaYmAdrinksYmAchair_plasty*2X~Dchair_plasty*3X~Dchair_plasty*4X~Dbar_polyfonY_Iplant_cruddyYmAbottleYmAbardesk_polyfonX~Dbardeskcorner_polyfonX~DfloortileHbar_armasY_Ibartable_armasYmAbar_chair_armasYmAcarpet_softZ@Kcarpet_soft*1Z@Kcarpet_soft*2Z@Kcarpet_soft*3Z@Kcarpet_soft*4Z@Kcarpet_soft*5Z@Kcarpet_soft*6Z@Kred_tvY_Iwood_tvYmAcarpet_polar*1Y_Ichair_plasty*5X~Dcarpet_polar*2Y_Icarpet_polar*3Y_Icarpet_polar*4Y_Ichair_plasty*6X~Dtable_polyfonYmAsmooth_table_polyfonYmAsofachair_polyfon_girlX~Dbed_polyfon_girl_one[dObed_polyfon_girlX~Dsofa_polyfon_girlZ[Mbed_budgetb_oneXQHbed_budgetbXQHplant_pineappleYmAplant_fruittreeY_Iplant_small_cactusY_Iplant_bonsaiY_Iplant_big_cactusY_Iplant_yukkaY_Icarpet_standard*7Y_Icarpet_standard*8Y_Icarpet_standard*9Y_Icarpet_standard*aY_Icarpet_standard*bY_Iplant_sunflowerY_Iplant_roseY_Itv_luxusY_IbathZ\BsinkY_ItoiletYmAduckYmAtileYmAtoilet_redYmAtoilet_yellYmAtile_redYmAtile_yellYmApresent_gen[~Npresent_gen1[~Npresent_gen2[~Npresent_gen3[~Npresent_gen4[~Npresent_gen5[~Npresent_gen6[~Nbar_basicY_Ishelves_basicXQHsoft_sofachair_norjaX~Dsoft_sofa_norjaX~Dlamp_basicXQHlamp2_armasYmAfridgeY_Idoor[dOdoorB[dOdoorC[dOpumpkinYmAskullcandleYmAdeadduckYmAdeadduck2YmAdeadduck3YmAmenorahYmApuddingYmAhamYmAturkeyYmAxmasduckY_IhouseYmAtriplecandleYmAtree3YmAtree4YmAtree5X~Dham2YmAwcandlesetYmArcandlesetYmAstatueYmAheartY_IvaleduckYmAheartsofaX~DthroneYmAsamovarY_IgiftflowersY_IhabbocakeYmAhologramYmAeasterduckY_IbunnyYmAbasketY_IbirdieYmAediceX~Dclub_sofaZ[Mprize1YmAprize2YmAprize3YmAdivider_poly3X~Ddivider_arm1YmAdivider_arm2YmAdivider_arm3YmAdivider_nor1X~Ddivider_silo1X~Ddivider_nor2X~Ddivider_silo2Z[Mdivider_nor3X~Ddivider_silo3X~DtypingmachineYmAspyroYmAredhologramYmAcameraHjoulutahtiYmAhyacinth1YmAhyacinth2YmAchair_plasto*10YmAchair_plasto*11YmAbardeskcorner_polyfon*12X~Dbardeskcorner_polyfon*13X~Dchair_plasto*12YmAchair_plasto*13YmAchair_plasto*14YmAtable_plasto_4leg*14Y_ImocchamasterY_Icarpet_legocourtYmAbench_legoYmAlegotrophyYmAvalentinescreenYmAedicehcYmArare_daffodil_rugYmArare_beehive_bulbY_IhcsohvaYmAhcammeYmArare_elephant_statueYmArare_fountainY_Irare_standYmArare_globeYmArare_hammockYmArare_elephant_statue*1YmArare_elephant_statue*2YmArare_fountain*1Y_Irare_fountain*2Y_Irare_fountain*3Y_Irare_beehive_bulb*1Y_Irare_beehive_bulb*2Y_Irare_xmas_screenY_Irare_parasol*1Y_Irare_parasol*2Y_Irare_parasol*3Y_Itree1X~Dtree2ZmBwcandleYxBrcandleYxBsoft_jaggara_norjaYmAhouse2YmAdjesko_turntableYmAmd_sofaZ[Mmd_limukaappiY_Itable_plasto_4leg*10Y_Itable_plasto_4leg*15Y_Itable_plasto_bigsquare*14Y_Itable_plasto_bigsquare*15Y_Itable_plasto_round*14Y_Itable_plasto_round*15Y_Itable_plasto_square*14Y_Itable_plasto_square*15Y_Ichair_plasto*15YmAchair_plasty*7X~Dchair_plasty*8X~Dchair_plasty*9X~Dchair_plasty*10X~Dchair_plasty*11X~Dchair_plasto*16YmAtable_plasto_4leg*16Y_Ihockey_scoreY_Ihockey_lightYmAdoorD[dOprizetrophy2*3[rIprizetrophy3*3XrIprizetrophy4*3[rIprizetrophy5*3[rIprizetrophy6*3[rIprizetrophy*1Y_Iprizetrophy2*1[rIprizetrophy3*1XrIprizetrophy4*1[rIprizetrophy5*1[rIprizetrophy6*1[rIprizetrophy*2Y_Iprizetrophy2*2[rIprizetrophy3*2XrIprizetrophy4*2[rIprizetrophy5*2[rIprizetrophy6*2[rIprizetrophy*3Y_Irare_parasol*0Hhc_lmp[fBhc_tblYmAhc_chrYmAhc_dskXQHnestHpetfood1ZvCpetfood2ZvCpetfood3ZvCwaterbowl*4XICwaterbowl*5XICwaterbowl*2XICwaterbowl*1XICwaterbowl*3XICtoy1XICtoy1*1XICtoy1*2XICtoy1*3XICtoy1*4XICgoodie1ZvCgoodie1*1ZvCgoodie1*2ZvCgoodie2X~Dprizetrophy7*3[rIprizetrophy7*1[rIprizetrophy7*2[rIscifiport*0Y_Iscifiport*9Y_Iscifiport*8Y_Iscifiport*7Y_Iscifiport*6Y_Iscifiport*5Y_Iscifiport*4Y_Iscifiport*3Y_Iscifiport*2Y_Iscifiport*1Y_Iscifirocket*9Y_Iscifirocket*8Y_Iscifirocket*7Y_Iscifirocket*6Y_Iscifirocket*5Y_Iscifirocket*4Y_Iscifirocket*3Y_Iscifirocket*2Y_Iscifirocket*1Y_Iscifirocket*0Y_Iscifidoor*10Y_Iscifidoor*9Y_Iscifidoor*8Y_Iscifidoor*7Y_Iscifidoor*6Y_Iscifidoor*5Y_Iscifidoor*4Y_Iscifidoor*3Y_Iscifidoor*2Y_Iscifidoor*1Y_Ipillow*5YmApillow*8YmApillow*0YmApillow*1YmApillow*2YmApillow*7YmApillow*9YmApillow*4YmApillow*6YmApillow*3YmAmarquee*1Y_Imarquee*2Y_Imarquee*7Y_Imarquee*aY_Imarquee*8Y_Imarquee*9Y_Imarquee*5Y_Imarquee*4Y_Imarquee*6Y_Imarquee*3Y_Iwooden_screen*1Y_Iwooden_screen*2Y_Iwooden_screen*7Y_Iwooden_screen*0Y_Iwooden_screen*8Y_Iwooden_screen*5Y_Iwooden_screen*9Y_Iwooden_screen*4Y_Iwooden_screen*6Y_Iwooden_screen*3Y_Ipillar*6Y_Ipillar*1Y_Ipillar*9Y_Ipillar*0Y_Ipillar*8Y_Ipillar*2Y_Ipillar*5Y_Ipillar*4Y_Ipillar*7Y_Ipillar*3Y_Irare_dragonlamp*4Y_Irare_dragonlamp*0Y_Irare_dragonlamp*5Y_Irare_dragonlamp*2Y_Irare_dragonlamp*8Y_Irare_dragonlamp*9Y_Irare_dragonlamp*7Y_Irare_dragonlamp*6Y_Irare_dragonlamp*1Y_Irare_dragonlamp*3Y_Irare_icecream*1Y_Irare_icecream*7Y_Irare_icecream*8Y_Irare_icecream*2Y_Irare_icecream*6Y_Irare_icecream*9Y_Irare_icecream*3Y_Irare_icecream*0Y_Irare_icecream*4Y_Irare_icecream*5Y_Irare_fan*7YxBrare_fan*6YxBrare_fan*9YxBrare_fan*3YxBrare_fan*0YxBrare_fan*4YxBrare_fan*5YxBrare_fan*1YxBrare_fan*8YxBrare_fan*2YxBqueue_tile1*3X~Dqueue_tile1*6X~Dqueue_tile1*4X~Dqueue_tile1*9X~Dqueue_tile1*8X~Dqueue_tile1*5X~Dqueue_tile1*7X~Dqueue_tile1*2X~Dqueue_tile1*1X~Dqueue_tile1*0X~DticketHrare_snowrugX~Dcn_lampZxIcn_sofaYmAsporttrack1*1YmAsporttrack1*3YmAsporttrack1*2YmAsporttrack2*1[~Nsporttrack2*2[~Nsporttrack2*3[~Nsporttrack3*1YmAsporttrack3*2YmAsporttrack3*3YmAfootylampX~Dbarchair_siloX~Ddivider_nor4*4X~Dtraffic_light*1ZxItraffic_light*2ZxItraffic_light*3ZxItraffic_light*4ZxItraffic_light*6ZxIrubberchair*1X~Drubberchair*2X~Drubberchair*3X~Drubberchair*4X~Drubberchair*5X~Drubberchair*6X~Dbarrier*1X~Dbarrier*2X~Dbarrier*3X~Drubberchair*7X~Drubberchair*8X~Dtable_norja_med*2Y_Itable_norja_med*3Y_Itable_norja_med*4Y_Itable_norja_med*5Y_Itable_norja_med*6Y_Itable_norja_med*7Y_Itable_norja_med*8Y_Itable_norja_med*9Y_Icouch_norja*2X~Dcouch_norja*3X~Dcouch_norja*4X~Dcouch_norja*5X~Dcouch_norja*6X~Dcouch_norja*7X~Dcouch_norja*8X~Dcouch_norja*9X~Dshelves_norja*2X~Dshelves_norja*3X~Dshelves_norja*4X~Dshelves_norja*5X~Dshelves_norja*6X~Dshelves_norja*7X~Dshelves_norja*8X~Dshelves_norja*9X~Dchair_norja*2X~Dchair_norja*3X~Dchair_norja*4X~Dchair_norja*5X~Dchair_norja*6X~Dchair_norja*7X~Dchair_norja*8X~Dchair_norja*9X~Ddivider_nor1*2X~Ddivider_nor1*3X~Ddivider_nor1*4X~Ddivider_nor1*5X~Ddivider_nor1*6X~Ddivider_nor1*7X~Ddivider_nor1*8X~Ddivider_nor1*9X~Dsoft_sofa_norja*2X~Dsoft_sofa_norja*3X~Dsoft_sofa_norja*4X~Dsoft_sofa_norja*5X~Dsoft_sofa_norja*6X~Dsoft_sofa_norja*7X~Dsoft_sofa_norja*8X~Dsoft_sofa_norja*9X~Dsoft_sofachair_norja*2X~Dsoft_sofachair_norja*3X~Dsoft_sofachair_norja*4X~Dsoft_sofachair_norja*5X~Dsoft_sofachair_norja*6X~Dsoft_sofachair_norja*7X~Dsoft_sofachair_norja*8X~Dsoft_sofachair_norja*9X~Dsofachair_silo*2X~Dsofachair_silo*3X~Dsofachair_silo*4X~Dsofachair_silo*5X~Dsofachair_silo*6X~Dsofachair_silo*7X~Dsofachair_silo*8X~Dsofachair_silo*9X~Dtable_silo_small*2X~Dtable_silo_small*3X~Dtable_silo_small*4X~Dtable_silo_small*5X~Dtable_silo_small*6X~Dtable_silo_small*7X~Dtable_silo_small*8X~Dtable_silo_small*9X~Ddivider_silo1*2X~Ddivider_silo1*3X~Ddivider_silo1*4X~Ddivider_silo1*5X~Ddivider_silo1*6X~Ddivider_silo1*7X~Ddivider_silo1*8X~Ddivider_silo1*9X~Ddivider_silo3*2X~Ddivider_silo3*3X~Ddivider_silo3*4X~Ddivider_silo3*5X~Ddivider_silo3*6X~Ddivider_silo3*7X~Ddivider_silo3*8X~Ddivider_silo3*9X~Dtable_silo_med*2X~Dtable_silo_med*3X~Dtable_silo_med*4X~Dtable_silo_med*5X~Dtable_silo_med*6X~Dtable_silo_med*7X~Dtable_silo_med*8X~Dtable_silo_med*9X~Dsofa_silo*2X~Dsofa_silo*3X~Dsofa_silo*4X~Dsofa_silo*5X~Dsofa_silo*6X~Dsofa_silo*7X~Dsofa_silo*8X~Dsofa_silo*9X~Dsofachair_polyfon*2X~Dsofachair_polyfon*3X~Dsofachair_polyfon*4X~Dsofachair_polyfon*6X~Dsofachair_polyfon*7X~Dsofachair_polyfon*8X~Dsofachair_polyfon*9X~Dsofa_polyfon*2Z[Msofa_polyfon*3Z[Msofa_polyfon*4Z[Msofa_polyfon*6Z[Msofa_polyfon*7Z[Msofa_polyfon*8Z[Msofa_polyfon*9Z[Mbed_polyfon*2X~Dbed_polyfon*3X~Dbed_polyfon*4X~Dbed_polyfon*6X~Dbed_polyfon*7X~Dbed_polyfon*8X~Dbed_polyfon*9X~Dbed_polyfon_one*2[dObed_polyfon_one*3[dObed_polyfon_one*4[dObed_polyfon_one*6[dObed_polyfon_one*7[dObed_polyfon_one*8[dObed_polyfon_one*9[dObardesk_polyfon*2X~Dbardesk_polyfon*3X~Dbardesk_polyfon*4X~Dbardesk_polyfon*5X~Dbardesk_polyfon*6X~Dbardesk_polyfon*7X~Dbardesk_polyfon*8X~Dbardesk_polyfon*9X~Dbardeskcorner_polyfon*2X~Dbardeskcorner_polyfon*3X~Dbardeskcorner_polyfon*4X~Dbardeskcorner_polyfon*5X~Dbardeskcorner_polyfon*6X~Dbardeskcorner_polyfon*7X~Dbardeskcorner_polyfon*8X~Dbardeskcorner_polyfon*9X~Ddivider_poly3*2X~Ddivider_poly3*3X~Ddivider_poly3*4X~Ddivider_poly3*5X~Ddivider_poly3*6X~Ddivider_poly3*7X~Ddivider_poly3*8X~Ddivider_poly3*9X~Dchair_silo*2X~Dchair_silo*3X~Dchair_silo*4X~Dchair_silo*5X~Dchair_silo*6X~Dchair_silo*7X~Dchair_silo*8X~Dchair_silo*9X~Ddivider_nor3*2X~Ddivider_nor3*3X~Ddivider_nor3*4X~Ddivider_nor3*5X~Ddivider_nor3*6X~Ddivider_nor3*7X~Ddivider_nor3*8X~Ddivider_nor3*9X~Ddivider_nor2*2X~Ddivider_nor2*3X~Ddivider_nor2*4X~Ddivider_nor2*5X~Ddivider_nor2*6X~Ddivider_nor2*7X~Ddivider_nor2*8X~Ddivider_nor2*9X~Dsilo_studydeskX~Dsolarium_norjaY_Isolarium_norja*1Y_Isolarium_norja*2Y_Isolarium_norja*3Y_Isolarium_norja*5Y_Isolarium_norja*6Y_Isolarium_norja*7Y_Isolarium_norja*8Y_Isolarium_norja*9Y_IsandrugX~Drare_moonrugYmAchair_chinaYmAchina_tableYmAsleepingbag*1YmAsleepingbag*2YmAsleepingbag*3YmAsleepingbag*4YmAsafe_siloY_Isleepingbag*7YmAsleepingbag*9YmAsleepingbag*5YmAsleepingbag*10YmAsleepingbag*6YmAsleepingbag*8YmAchina_shelveX~Dtraffic_light*5ZxIdivider_nor4*2X~Ddivider_nor4*3X~Ddivider_nor4*5X~Ddivider_nor4*6X~Ddivider_nor4*7X~Ddivider_nor4*8X~Ddivider_nor4*9X~Ddivider_nor5*2X~Ddivider_nor5*3X~Ddivider_nor5*4X~Ddivider_nor5*5X~Ddivider_nor5*6X~Ddivider_nor5*7X~Ddivider_nor5*8X~Ddivider_nor5*9X~Ddivider_nor5X~Ddivider_nor4X~Dwall_chinaYmAcorner_chinaYmAbarchair_silo*2X~Dbarchair_silo*3X~Dbarchair_silo*4X~Dbarchair_silo*5X~Dbarchair_silo*6X~Dbarchair_silo*7X~Dbarchair_silo*8X~Dbarchair_silo*9X~Dsafe_silo*2Y_Isafe_silo*3Y_Isafe_silo*4Y_Isafe_silo*5Y_Isafe_silo*6Y_Isafe_silo*7Y_Isafe_silo*8Y_Isafe_silo*9Y_Iglass_shelfY_Iglass_chairY_Iglass_stoolY_Iglass_sofaY_Iglass_tableY_Iglass_table*2Y_Iglass_table*3Y_Iglass_table*4Y_Iglass_table*5Y_Iglass_table*6Y_Iglass_table*7Y_Iglass_table*8Y_Iglass_table*9Y_Iglass_chair*2Y_Iglass_chair*3Y_Iglass_chair*4Y_Iglass_chair*5Y_Iglass_chair*6Y_Iglass_chair*7Y_Iglass_chair*8Y_Iglass_chair*9Y_Iglass_sofa*2Y_Iglass_sofa*3Y_Iglass_sofa*4Y_Iglass_sofa*5Y_Iglass_sofa*6Y_Iglass_sofa*7Y_Iglass_sofa*8Y_Iglass_sofa*9Y_Iglass_stool*2Y_Iglass_stool*4Y_Iglass_stool*5Y_Iglass_stool*6Y_Iglass_stool*7Y_Iglass_stool*8Y_Iglass_stool*3Y_Iglass_stool*9Y_ICFC_100_coin_goldZvCCFC_10_coin_bronzeZvCCFC_200_moneybagZvCCFC_500_goldbarZvCCFC_50_coin_silverZvCCF_10_coin_goldZvCCF_1_coin_bronzeZvCCF_20_moneybagZvCCF_50_goldbarZvCCF_5_coin_silverZvChc_crptYmAhc_tvZ\BgothgateX~DgothiccandelabraYxBgothrailingX~Dgoth_tableYmAhc_bkshlfYmAhc_btlrY_Ihc_crtnYmAhc_djsetYmAhc_frplcZbBhc_lmpstYmAhc_machineYmAhc_rllrXQHhc_rntgnX~Dhc_trllYmAgothic_chair*1X~Dgothic_sofa*1X~Dgothic_stool*1X~Dgothic_chair*2X~Dgothic_sofa*2X~Dgothic_stool*2X~Dgothic_chair*3X~Dgothic_sofa*3X~Dgothic_stool*3X~Dgothic_chair*4X~Dgothic_sofa*4X~Dgothic_stool*4X~Dgothic_chair*5X~Dgothic_sofa*5X~Dgothic_stool*5X~Dgothic_chair*6X~Dgothic_sofa*6X~Dgothic_stool*6X~Dval_cauldronX~Dsound_machineX~Dromantique_pianochair*3Y_Iromantique_pianochair*5Y_Iromantique_pianochair*2Y_Iromantique_pianochair*4Y_Iromantique_pianochair*1Y_Iromantique_divan*3Y_Iromantique_divan*5Y_Iromantique_divan*2Y_Iromantique_divan*4Y_Iromantique_divan*1Y_Iromantique_chair*3Y_Iromantique_chair*5Y_Iromantique_chair*2Y_Iromantique_chair*4Y_Iromantique_chair*1Y_Irare_parasolY_Iplant_valentinerose*3XICplant_valentinerose*5XICplant_valentinerose*2XICplant_valentinerose*4XICplant_valentinerose*1XICplant_mazegateYeCplant_mazeZcCplant_bulrushXICpetfood4Y_Icarpet_valentineZ|Egothic_carpetXICgothic_carpet2Z|Egothic_chairX~Dgothic_sofaX~Dgothic_stoolX~Dgrand_piano*3Z|Egrand_piano*5Z|Egrand_piano*2Z|Egrand_piano*4Z|Egrand_piano*1Z|Etheatre_seatZ@Kromantique_tray2Y_Iromantique_tray1Y_Iromantique_smalltabl*3Y_Iromantique_smalltabl*5Y_Iromantique_smalltabl*2Y_Iromantique_smalltabl*4Y_Iromantique_smalltabl*1Y_Iromantique_mirrortablY_Iromantique_divider*3Z[Mromantique_divider*2Z[Mromantique_divider*4Z[Mromantique_divider*1Z[Mjp_tatami2YGGjp_tatamiYGGhabbowood_chairYGGjp_bambooYGGjp_iroriXQHjp_pillowYGGsound_set_1Y_Isound_set_2Y_Isound_set_3Y_Isound_set_4Y_Isound_set_5Z@Ksound_set_6Y_Isound_set_7Y_Isound_set_8Y_Isound_set_9Y_Isound_machine*1ZIPspotlightY_Isound_machine*2ZIPsound_machine*3ZIPsound_machine*4ZIPsound_machine*5ZIPsound_machine*6ZIPsound_machine*7ZIProm_lampZ|Erclr_sofaXQHrclr_gardenXQHrclr_chairZ|Esound_set_28Y_Isound_set_27Y_Isound_set_26Y_Isound_set_25Y_Isound_set_24Y_Isound_set_23Y_Isound_set_22Y_Isound_set_21Y_Isound_set_20Z@Ksound_set_19Z@Ksound_set_18Y_Isound_set_17Y_Isound_set_16Y_Isound_set_15Y_Isound_set_14Y_Isound_set_13Y_Isound_set_12Y_Isound_set_11Y_Isound_set_10Y_Irope_dividerXQHromantique_clockY_Irare_icecream_campaignY_Ipura_mdl5*1XQHpura_mdl5*2XQHpura_mdl5*3XQHpura_mdl5*4XQHpura_mdl5*5XQHpura_mdl5*6XQHpura_mdl5*7XQHpura_mdl5*8XQHpura_mdl5*9XQHpura_mdl4*1XQHpura_mdl4*2XQHpura_mdl4*3XQHpura_mdl4*4XQHpura_mdl4*5XQHpura_mdl4*6XQHpura_mdl4*7XQHpura_mdl4*8XQHpura_mdl4*9XQHpura_mdl3*1XQHpura_mdl3*2XQHpura_mdl3*3XQHpura_mdl3*4XQHpura_mdl3*5XQHpura_mdl3*6XQHpura_mdl3*7XQHpura_mdl3*8XQHpura_mdl3*9XQHpura_mdl2*1XQHpura_mdl2*2XQHpura_mdl2*3XQHpura_mdl2*4XQHpura_mdl2*5XQHpura_mdl2*6XQHpura_mdl2*7XQHpura_mdl2*8XQHpura_mdl2*9XQHpura_mdl1*1XQHpura_mdl1*2XQHpura_mdl1*3XQHpura_mdl1*4XQHpura_mdl1*5XQHpura_mdl1*6XQHpura_mdl1*7XQHpura_mdl1*8XQHpura_mdl1*9XQHjp_lanternXQHchair_basic*1XQHchair_basic*2XQHchair_basic*3XQHchair_basic*4XQHchair_basic*5XQHchair_basic*6XQHchair_basic*7XQHchair_basic*8XQHchair_basic*9XQHbed_budget*1XQHbed_budget*2XQHbed_budget*3XQHbed_budget*4XQHbed_budget*5XQHbed_budget*6XQHbed_budget*7XQHbed_budget*8XQHbed_budget*9XQHbed_budget_one*1XQHbed_budget_one*2XQHbed_budget_one*3XQHbed_budget_one*4XQHbed_budget_one*5XQHbed_budget_one*6XQHbed_budget_one*7XQHbed_budget_one*8XQHbed_budget_one*9XQHjp_drawerXQHtile_stellaZ[Mtile_marbleZ[Mtile_brownZ[Msummer_grill*1Y_Isummer_grill*2Y_Isummer_grill*3Y_Isummer_grill*4Y_Isummer_chair*1Y_Isummer_chair*2Y_Isummer_chair*3Y_Isummer_chair*4Y_Isummer_chair*5Y_Isummer_chair*6Y_Isummer_chair*7Y_Isummer_chair*8Y_Isummer_chair*9Y_Isound_set_36ZfIsound_set_35ZfIsound_set_34ZfIsound_set_33ZfIsound_set_32Y_Isound_set_31Y_Isound_set_30Y_Isound_set_29Y_Isound_machine_pro[~Nrare_mnstrY_Ione_way_door*1XQHone_way_door*2XQHone_way_door*3XQHone_way_door*4XQHone_way_door*5XQHone_way_door*6XQHone_way_door*7XQHone_way_door*8XQHone_way_door*9XQHexe_rugZ[Mexe_s_tableZGRsound_set_37ZfIsummer_pool*1ZlIsummer_pool*2ZlIsummer_pool*3ZlIsummer_pool*4ZlIsong_diskY_Ijukebox*1[~Ncarpet_soft_tut[~Nsound_set_44Z@Ksound_set_43Z@Ksound_set_42Z@Ksound_set_41Z@Ksound_set_40Z@Ksound_set_39Z@Ksound_set_38Z@Kgrunge_chairZ@Kgrunge_mattressZ@Kgrunge_radiatorZ@Kgrunge_shelfZ@Kgrunge_signZ@Kgrunge_tableZ@Khabboween_crypt[uKhabboween_grassZ@Khal_cauldronZ@Khal_graveZ@Ksound_set_52ZuKsound_set_51ZuKsound_set_50ZuKsound_set_49ZuKsound_set_48ZuKsound_set_47ZuKsound_set_46ZuKsound_set_45ZuKxmas_icelampZ[Mxmas_cstl_wallZ[Mxmas_cstl_twrZ[Mxmas_cstl_gate[~Ntree7Z[Mtree6Z[Msound_set_54Z[Msound_set_53Z[Msafe_silo_pb[dOplant_mazegate_snowZ[Mplant_maze_snowZ[Mchristmas_sleighZ[Mchristmas_reindeer[~Nchristmas_poopZ[Mexe_bardeskZ[Mexe_chairZ[Mexe_chair2Z[Mexe_cornerZ[Mexe_drinksZ[Mexe_sofaZ[Mexe_tableZ[Msound_set_59[~Nsound_set_58[~Nsound_set_57[~Nsound_set_56[~Nsound_set_55[~Nnoob_table*1[~Nnoob_table*2[~Nnoob_table*3[~Nnoob_table*4[~Nnoob_table*5[~Nnoob_table*6[~Nnoob_stool*1[~Nnoob_stool*2[~Nnoob_stool*3[~Nnoob_stool*4[~Nnoob_stool*5[~Nnoob_stool*6[~Nnoob_rug*1[~Nnoob_rug*2[~Nnoob_rug*3[~Nnoob_rug*4[~Nnoob_rug*5[~Nnoob_rug*6[~Nnoob_lamp*1[dOnoob_lamp*2[dOnoob_lamp*3[dOnoob_lamp*4[dOnoob_lamp*5[dOnoob_lamp*6[dOnoob_chair*1[~Nnoob_chair*2[~Nnoob_chair*3[~Nnoob_chair*4[~Nnoob_chair*5[~Nnoob_chair*6[~Nexe_globe[~Nexe_plantZ[Mval_teddy*1[dOval_teddy*2[dOval_teddy*3[dOval_teddy*4[dOval_teddy*5[dOval_teddy*6[dOval_randomizer[dOval_choco[dOteleport_door[dOsound_set_61[dOsound_set_60[dOfortune[dOsw_tableZIPsw_raven[cQsw_chestZIPsand_cstl_wallZIPsand_cstl_twrZIPsand_cstl_gateZIPgrunge_candleZIPgrunge_benchZIPgrunge_barrelZIPrclr_lampZGRprizetrophy9*1ZGRprizetrophy8*1ZGRnouvelle_traxYcPmd_rugZGRjp_tray6ZGRjp_tray5ZGRjp_tray4ZGRjp_tray3ZGRjp_tray2ZGRjp_tray1ZGRarabian_teamkZGRarabian_snakeZGRarabian_rugZGRarabian_pllwZGRarabian_divdrZGRarabian_chairZGRarabian_bigtbZGRarabian_tetblZGRarabian_tray1ZGRarabian_tray2ZGRarabian_tray3ZGRarabian_tray4ZGRPIpost.itHpost.it.vdHphotoHChessHTicTacToeHBattleShipHPokerHwallpaperHfloorHposterZ@KgothicfountainYxBhc_wall_lampZbBindustrialfanZ`BtorchZ\Bval_heartXBCwallmirrorZ|Ejp_ninjastarsXQHhabw_mirrorXQHhabbowheelZ[Mguitar_skullZ@Kguitar_vZ@Kxmas_light[~Nhrella_poster_3[Nhrella_poster_2ZIPhrella_poster_1[Nsw_swordsZIPsw_stoneZIPsw_holeZIProomdimmerZGRmd_logo_wallZGRmd_canZGRjp_sheet3ZGRjp_sheet2ZGRjp_sheet1ZGRarabian_swordsZGRarabian_wndwZGR")
                        receivedItemIndex = True
                    End If
                End If

            Case "A@" '// Enter room - add this user to the room and get refreshed statuses of other users (good rotation, sitting etc)
                roomCommunicator.enterUser(userDetails)

                'Case "bbShizzle" '// ._.
                '   userDetails.inBBLobby = False
                '  resetGameStatuses()

                ' If userDetails.inPublicroom = True And userDetails.roomID = 17 Then
                'If IsNothing(HoloBBGAMELOBBY) Then HoloBBGAMELOBBY = New clsHoloBBGAMELOBBY(roomCommunicator)
                'userDetails.inBBLobby = True
                'transData("Cg" & HoloENCODING.encodeVL64(0) & "BattleBall leet" & sysChar(2) & HoloENCODING.encodeVL64(1000000) & sysChar(1))
                'End If


            Case "AO" '// User looks to something (rotate body + head)
                If userDetails.containsStatus("sit") = True Then Return
                Dim ToX As Integer = currentPacket.Substring(2).Split(" ")(0)
                Dim ToY As Integer = currentPacket.Split(" ")(1)
                With userDetails
                    If .PosY > ToY Then .rotHead = 0
                    If .PosX < ToX Then .rotHead = 2
                    If .PosY < ToY Then .rotHead = 4
                    If .PosX > ToX Then .rotHead = 6
                    If .PosX < ToX And .PosY > ToY Then .rotHead = 1
                    If .PosX < ToX And .PosY < ToY Then .rotHead = 3
                    If .PosX > ToX And .PosY < ToY Then .rotHead = 5
                    If .PosX > ToX And .PosY > ToY Then .rotHead = 7
                    .rotBody = .rotHead
                End With
                roomCommunicator.refreshUser(userDetails)

            Case "CW" '// Sprite index
                If receivedItemIndex = False Then
                    transData("Dg[_Dshelves_norjaX~Dshelves_polyfonYmAshelves_siloXQHtable_polyfon_smallYmAchair_polyfonZbBtable_norja_medY_Itable_silo_medX~Dtable_plasto_4legY_Itable_plasto_roundY_Itable_plasto_bigsquareY_Istand_polyfon_zZbBchair_siloX~Dsofa_siloX~Dcouch_norjaX~Dchair_norjaX~Dtable_polyfon_medYmAdoormat_loveZbBdoormat_plainZ[Msofachair_polyfonX~Dsofa_polyfonZ[Msofachair_siloX~Dchair_plastyX~Dchair_plastoYmAtable_plasto_squareY_Ibed_polyfonX~Dbed_polyfon_one[dObed_trad_oneYmAbed_tradYmAbed_silo_oneYmAbed_silo_twoYmAtable_silo_smallX~Dbed_armas_twoYmAbed_budget_oneXQHbed_budgetXQHshelves_armasYmAbench_armasYmAtable_armasYmAsmall_table_armasZbBsmall_chair_armasYmAfireplace_armasYmAlamp_armasYmAbed_armas_oneYmAcarpet_standardY_Icarpet_armasYmAcarpet_polarY_Ifireplace_polyfonY_Itable_plasto_4leg*1Y_Itable_plasto_bigsquare*1Y_Itable_plasto_round*1Y_Itable_plasto_square*1Y_Ichair_plasto*1YmAcarpet_standard*1Y_Idoormat_plain*1Z[Mtable_plasto_4leg*2Y_Itable_plasto_bigsquare*2Y_Itable_plasto_round*2Y_Itable_plasto_square*2Y_Ichair_plasto*2YmAdoormat_plain*2Z[Mcarpet_standard*2Y_Itable_plasto_4leg*3Y_Itable_plasto_bigsquare*3Y_Itable_plasto_round*3Y_Itable_plasto_square*3Y_Ichair_plasto*3YmAcarpet_standard*3Y_Idoormat_plain*3Z[Mtable_plasto_4leg*4Y_Itable_plasto_bigsquare*4Y_Itable_plasto_round*4Y_Itable_plasto_square*4Y_Ichair_plasto*4YmAcarpet_standard*4Y_Idoormat_plain*4Z[Mdoormat_plain*6Z[Mdoormat_plain*5Z[Mcarpet_standard*5Y_Itable_plasto_4leg*5Y_Itable_plasto_bigsquare*5Y_Itable_plasto_round*5Y_Itable_plasto_square*5Y_Ichair_plasto*5YmAtable_plasto_4leg*6Y_Itable_plasto_bigsquare*6Y_Itable_plasto_round*6Y_Itable_plasto_square*6Y_Ichair_plasto*6YmAtable_plasto_4leg*7Y_Itable_plasto_bigsquare*7Y_Itable_plasto_round*7Y_Itable_plasto_square*7Y_Ichair_plasto*7YmAtable_plasto_4leg*8Y_Itable_plasto_bigsquare*8Y_Itable_plasto_round*8Y_Itable_plasto_square*8Y_Ichair_plasto*8YmAtable_plasto_4leg*9Y_Itable_plasto_bigsquare*9Y_Itable_plasto_round*9Y_Itable_plasto_square*9Y_Ichair_plasto*9YmAcarpet_standard*6Y_Ichair_plasty*1X~DpizzaYmAdrinksYmAchair_plasty*2X~Dchair_plasty*3X~Dchair_plasty*4X~Dbar_polyfonY_Iplant_cruddyYmAbottleYmAbardesk_polyfonX~Dbardeskcorner_polyfonX~DfloortileHbar_armasY_Ibartable_armasYmAbar_chair_armasYmAcarpet_softZ@Kcarpet_soft*1Z@Kcarpet_soft*2Z@Kcarpet_soft*3Z@Kcarpet_soft*4Z@Kcarpet_soft*5Z@Kcarpet_soft*6Z@Kred_tvY_Iwood_tvYmAcarpet_polar*1Y_Ichair_plasty*5X~Dcarpet_polar*2Y_Icarpet_polar*3Y_Icarpet_polar*4Y_Ichair_plasty*6X~Dtable_polyfonYmAsmooth_table_polyfonYmAsofachair_polyfon_girlX~Dbed_polyfon_girl_one[dObed_polyfon_girlX~Dsofa_polyfon_girlZ[Mbed_budgetb_oneXQHbed_budgetbXQHplant_pineappleYmAplant_fruittreeY_Iplant_small_cactusY_Iplant_bonsaiY_Iplant_big_cactusY_Iplant_yukkaY_Icarpet_standard*7Y_Icarpet_standard*8Y_Icarpet_standard*9Y_Icarpet_standard*aY_Icarpet_standard*bY_Iplant_sunflowerY_Iplant_roseY_Itv_luxusY_IbathZ\BsinkY_ItoiletYmAduckYmAtileYmAtoilet_redYmAtoilet_yellYmAtile_redYmAtile_yellYmApresent_gen[~Npresent_gen1[~Npresent_gen2[~Npresent_gen3[~Npresent_gen4[~Npresent_gen5[~Npresent_gen6[~Nbar_basicY_Ishelves_basicXQHsoft_sofachair_norjaX~Dsoft_sofa_norjaX~Dlamp_basicXQHlamp2_armasYmAfridgeY_Idoor[dOdoorB[dOdoorC[dOpumpkinYmAskullcandleYmAdeadduckYmAdeadduck2YmAdeadduck3YmAmenorahYmApuddingYmAhamYmAturkeyYmAxmasduckY_IhouseYmAtriplecandleYmAtree3YmAtree4YmAtree5X~Dham2YmAwcandlesetYmArcandlesetYmAstatueYmAheartY_IvaleduckYmAheartsofaX~DthroneYmAsamovarY_IgiftflowersY_IhabbocakeYmAhologramYmAeasterduckY_IbunnyYmAbasketY_IbirdieYmAediceX~Dclub_sofaZ[Mprize1YmAprize2YmAprize3YmAdivider_poly3X~Ddivider_arm1YmAdivider_arm2YmAdivider_arm3YmAdivider_nor1X~Ddivider_silo1X~Ddivider_nor2X~Ddivider_silo2Z[Mdivider_nor3X~Ddivider_silo3X~DtypingmachineYmAspyroYmAredhologramYmAcameraHjoulutahtiYmAhyacinth1YmAhyacinth2YmAchair_plasto*10YmAchair_plasto*11YmAbardeskcorner_polyfon*12X~Dbardeskcorner_polyfon*13X~Dchair_plasto*12YmAchair_plasto*13YmAchair_plasto*14YmAtable_plasto_4leg*14Y_ImocchamasterY_Icarpet_legocourtYmAbench_legoYmAlegotrophyYmAvalentinescreenYmAedicehcYmArare_daffodil_rugYmArare_beehive_bulbY_IhcsohvaYmAhcammeYmArare_elephant_statueYmArare_fountainY_Irare_standYmArare_globeYmArare_hammockYmArare_elephant_statue*1YmArare_elephant_statue*2YmArare_fountain*1Y_Irare_fountain*2Y_Irare_fountain*3Y_Irare_beehive_bulb*1Y_Irare_beehive_bulb*2Y_Irare_xmas_screenY_Irare_parasol*1Y_Irare_parasol*2Y_Irare_parasol*3Y_Itree1X~Dtree2ZmBwcandleYxBrcandleYxBsoft_jaggara_norjaYmAhouse2YmAdjesko_turntableYmAmd_sofaZ[Mmd_limukaappiY_Itable_plasto_4leg*10Y_Itable_plasto_4leg*15Y_Itable_plasto_bigsquare*14Y_Itable_plasto_bigsquare*15Y_Itable_plasto_round*14Y_Itable_plasto_round*15Y_Itable_plasto_square*14Y_Itable_plasto_square*15Y_Ichair_plasto*15YmAchair_plasty*7X~Dchair_plasty*8X~Dchair_plasty*9X~Dchair_plasty*10X~Dchair_plasty*11X~Dchair_plasto*16YmAtable_plasto_4leg*16Y_Ihockey_scoreY_Ihockey_lightYmAdoorD[dOprizetrophy2*3[rIprizetrophy3*3XrIprizetrophy4*3[rIprizetrophy5*3[rIprizetrophy6*3[rIprizetrophy*1Y_Iprizetrophy2*1[rIprizetrophy3*1XrIprizetrophy4*1[rIprizetrophy5*1[rIprizetrophy6*1[rIprizetrophy*2Y_Iprizetrophy2*2[rIprizetrophy3*2XrIprizetrophy4*2[rIprizetrophy5*2[rIprizetrophy6*2[rIprizetrophy*3Y_Irare_parasol*0Hhc_lmp[fBhc_tblYmAhc_chrYmAhc_dskXQHnestHpetfood1ZvCpetfood2ZvCpetfood3ZvCwaterbowl*4XICwaterbowl*5XICwaterbowl*2XICwaterbowl*1XICwaterbowl*3XICtoy1XICtoy1*1XICtoy1*2XICtoy1*3XICtoy1*4XICgoodie1ZvCgoodie1*1ZvCgoodie1*2ZvCgoodie2X~Dprizetrophy7*3[rIprizetrophy7*1[rIprizetrophy7*2[rIscifiport*0Y_Iscifiport*9Y_Iscifiport*8Y_Iscifiport*7Y_Iscifiport*6Y_Iscifiport*5Y_Iscifiport*4Y_Iscifiport*3Y_Iscifiport*2Y_Iscifiport*1Y_Iscifirocket*9Y_Iscifirocket*8Y_Iscifirocket*7Y_Iscifirocket*6Y_Iscifirocket*5Y_Iscifirocket*4Y_Iscifirocket*3Y_Iscifirocket*2Y_Iscifirocket*1Y_Iscifirocket*0Y_Iscifidoor*10Y_Iscifidoor*9Y_Iscifidoor*8Y_Iscifidoor*7Y_Iscifidoor*6Y_Iscifidoor*5Y_Iscifidoor*4Y_Iscifidoor*3Y_Iscifidoor*2Y_Iscifidoor*1Y_Ipillow*5YmApillow*8YmApillow*0YmApillow*1YmApillow*2YmApillow*7YmApillow*9YmApillow*4YmApillow*6YmApillow*3YmAmarquee*1Y_Imarquee*2Y_Imarquee*7Y_Imarquee*aY_Imarquee*8Y_Imarquee*9Y_Imarquee*5Y_Imarquee*4Y_Imarquee*6Y_Imarquee*3Y_Iwooden_screen*1Y_Iwooden_screen*2Y_Iwooden_screen*7Y_Iwooden_screen*0Y_Iwooden_screen*8Y_Iwooden_screen*5Y_Iwooden_screen*9Y_Iwooden_screen*4Y_Iwooden_screen*6Y_Iwooden_screen*3Y_Ipillar*6Y_Ipillar*1Y_Ipillar*9Y_Ipillar*0Y_Ipillar*8Y_Ipillar*2Y_Ipillar*5Y_Ipillar*4Y_Ipillar*7Y_Ipillar*3Y_Irare_dragonlamp*4Y_Irare_dragonlamp*0Y_Irare_dragonlamp*5Y_Irare_dragonlamp*2Y_Irare_dragonlamp*8Y_Irare_dragonlamp*9Y_Irare_dragonlamp*7Y_Irare_dragonlamp*6Y_Irare_dragonlamp*1Y_Irare_dragonlamp*3Y_Irare_icecream*1Y_Irare_icecream*7Y_Irare_icecream*8Y_Irare_icecream*2Y_Irare_icecream*6Y_Irare_icecream*9Y_Irare_icecream*3Y_Irare_icecream*0Y_Irare_icecream*4Y_Irare_icecream*5Y_Irare_fan*7YxBrare_fan*6YxBrare_fan*9YxBrare_fan*3YxBrare_fan*0YxBrare_fan*4YxBrare_fan*5YxBrare_fan*1YxBrare_fan*8YxBrare_fan*2YxBqueue_tile1*3X~Dqueue_tile1*6X~Dqueue_tile1*4X~Dqueue_tile1*9X~Dqueue_tile1*8X~Dqueue_tile1*5X~Dqueue_tile1*7X~Dqueue_tile1*2X~Dqueue_tile1*1X~Dqueue_tile1*0X~DticketHrare_snowrugX~Dcn_lampZxIcn_sofaYmAsporttrack1*1YmAsporttrack1*3YmAsporttrack1*2YmAsporttrack2*1[~Nsporttrack2*2[~Nsporttrack2*3[~Nsporttrack3*1YmAsporttrack3*2YmAsporttrack3*3YmAfootylampX~Dbarchair_siloX~Ddivider_nor4*4X~Dtraffic_light*1ZxItraffic_light*2ZxItraffic_light*3ZxItraffic_light*4ZxItraffic_light*6ZxIrubberchair*1X~Drubberchair*2X~Drubberchair*3X~Drubberchair*4X~Drubberchair*5X~Drubberchair*6X~Dbarrier*1X~Dbarrier*2X~Dbarrier*3X~Drubberchair*7X~Drubberchair*8X~Dtable_norja_med*2Y_Itable_norja_med*3Y_Itable_norja_med*4Y_Itable_norja_med*5Y_Itable_norja_med*6Y_Itable_norja_med*7Y_Itable_norja_med*8Y_Itable_norja_med*9Y_Icouch_norja*2X~Dcouch_norja*3X~Dcouch_norja*4X~Dcouch_norja*5X~Dcouch_norja*6X~Dcouch_norja*7X~Dcouch_norja*8X~Dcouch_norja*9X~Dshelves_norja*2X~Dshelves_norja*3X~Dshelves_norja*4X~Dshelves_norja*5X~Dshelves_norja*6X~Dshelves_norja*7X~Dshelves_norja*8X~Dshelves_norja*9X~Dchair_norja*2X~Dchair_norja*3X~Dchair_norja*4X~Dchair_norja*5X~Dchair_norja*6X~Dchair_norja*7X~Dchair_norja*8X~Dchair_norja*9X~Ddivider_nor1*2X~Ddivider_nor1*3X~Ddivider_nor1*4X~Ddivider_nor1*5X~Ddivider_nor1*6X~Ddivider_nor1*7X~Ddivider_nor1*8X~Ddivider_nor1*9X~Dsoft_sofa_norja*2X~Dsoft_sofa_norja*3X~Dsoft_sofa_norja*4X~Dsoft_sofa_norja*5X~Dsoft_sofa_norja*6X~Dsoft_sofa_norja*7X~Dsoft_sofa_norja*8X~Dsoft_sofa_norja*9X~Dsoft_sofachair_norja*2X~Dsoft_sofachair_norja*3X~Dsoft_sofachair_norja*4X~Dsoft_sofachair_norja*5X~Dsoft_sofachair_norja*6X~Dsoft_sofachair_norja*7X~Dsoft_sofachair_norja*8X~Dsoft_sofachair_norja*9X~Dsofachair_silo*2X~Dsofachair_silo*3X~Dsofachair_silo*4X~Dsofachair_silo*5X~Dsofachair_silo*6X~Dsofachair_silo*7X~Dsofachair_silo*8X~Dsofachair_silo*9X~Dtable_silo_small*2X~Dtable_silo_small*3X~Dtable_silo_small*4X~Dtable_silo_small*5X~Dtable_silo_small*6X~Dtable_silo_small*7X~Dtable_silo_small*8X~Dtable_silo_small*9X~Ddivider_silo1*2X~Ddivider_silo1*3X~Ddivider_silo1*4X~Ddivider_silo1*5X~Ddivider_silo1*6X~Ddivider_silo1*7X~Ddivider_silo1*8X~Ddivider_silo1*9X~Ddivider_silo3*2X~Ddivider_silo3*3X~Ddivider_silo3*4X~Ddivider_silo3*5X~Ddivider_silo3*6X~Ddivider_silo3*7X~Ddivider_silo3*8X~Ddivider_silo3*9X~Dtable_silo_med*2X~Dtable_silo_med*3X~Dtable_silo_med*4X~Dtable_silo_med*5X~Dtable_silo_med*6X~Dtable_silo_med*7X~Dtable_silo_med*8X~Dtable_silo_med*9X~Dsofa_silo*2X~Dsofa_silo*3X~Dsofa_silo*4X~Dsofa_silo*5X~Dsofa_silo*6X~Dsofa_silo*7X~Dsofa_silo*8X~Dsofa_silo*9X~Dsofachair_polyfon*2X~Dsofachair_polyfon*3X~Dsofachair_polyfon*4X~Dsofachair_polyfon*6X~Dsofachair_polyfon*7X~Dsofachair_polyfon*8X~Dsofachair_polyfon*9X~Dsofa_polyfon*2Z[Msofa_polyfon*3Z[Msofa_polyfon*4Z[Msofa_polyfon*6Z[Msofa_polyfon*7Z[Msofa_polyfon*8Z[Msofa_polyfon*9Z[Mbed_polyfon*2X~Dbed_polyfon*3X~Dbed_polyfon*4X~Dbed_polyfon*6X~Dbed_polyfon*7X~Dbed_polyfon*8X~Dbed_polyfon*9X~Dbed_polyfon_one*2[dObed_polyfon_one*3[dObed_polyfon_one*4[dObed_polyfon_one*6[dObed_polyfon_one*7[dObed_polyfon_one*8[dObed_polyfon_one*9[dObardesk_polyfon*2X~Dbardesk_polyfon*3X~Dbardesk_polyfon*4X~Dbardesk_polyfon*5X~Dbardesk_polyfon*6X~Dbardesk_polyfon*7X~Dbardesk_polyfon*8X~Dbardesk_polyfon*9X~Dbardeskcorner_polyfon*2X~Dbardeskcorner_polyfon*3X~Dbardeskcorner_polyfon*4X~Dbardeskcorner_polyfon*5X~Dbardeskcorner_polyfon*6X~Dbardeskcorner_polyfon*7X~Dbardeskcorner_polyfon*8X~Dbardeskcorner_polyfon*9X~Ddivider_poly3*2X~Ddivider_poly3*3X~Ddivider_poly3*4X~Ddivider_poly3*5X~Ddivider_poly3*6X~Ddivider_poly3*7X~Ddivider_poly3*8X~Ddivider_poly3*9X~Dchair_silo*2X~Dchair_silo*3X~Dchair_silo*4X~Dchair_silo*5X~Dchair_silo*6X~Dchair_silo*7X~Dchair_silo*8X~Dchair_silo*9X~Ddivider_nor3*2X~Ddivider_nor3*3X~Ddivider_nor3*4X~Ddivider_nor3*5X~Ddivider_nor3*6X~Ddivider_nor3*7X~Ddivider_nor3*8X~Ddivider_nor3*9X~Ddivider_nor2*2X~Ddivider_nor2*3X~Ddivider_nor2*4X~Ddivider_nor2*5X~Ddivider_nor2*6X~Ddivider_nor2*7X~Ddivider_nor2*8X~Ddivider_nor2*9X~Dsilo_studydeskX~Dsolarium_norjaY_Isolarium_norja*1Y_Isolarium_norja*2Y_Isolarium_norja*3Y_Isolarium_norja*5Y_Isolarium_norja*6Y_Isolarium_norja*7Y_Isolarium_norja*8Y_Isolarium_norja*9Y_IsandrugX~Drare_moonrugYmAchair_chinaYmAchina_tableYmAsleepingbag*1YmAsleepingbag*2YmAsleepingbag*3YmAsleepingbag*4YmAsafe_siloY_Isleepingbag*7YmAsleepingbag*9YmAsleepingbag*5YmAsleepingbag*10YmAsleepingbag*6YmAsleepingbag*8YmAchina_shelveX~Dtraffic_light*5ZxIdivider_nor4*2X~Ddivider_nor4*3X~Ddivider_nor4*5X~Ddivider_nor4*6X~Ddivider_nor4*7X~Ddivider_nor4*8X~Ddivider_nor4*9X~Ddivider_nor5*2X~Ddivider_nor5*3X~Ddivider_nor5*4X~Ddivider_nor5*5X~Ddivider_nor5*6X~Ddivider_nor5*7X~Ddivider_nor5*8X~Ddivider_nor5*9X~Ddivider_nor5X~Ddivider_nor4X~Dwall_chinaYmAcorner_chinaYmAbarchair_silo*2X~Dbarchair_silo*3X~Dbarchair_silo*4X~Dbarchair_silo*5X~Dbarchair_silo*6X~Dbarchair_silo*7X~Dbarchair_silo*8X~Dbarchair_silo*9X~Dsafe_silo*2Y_Isafe_silo*3Y_Isafe_silo*4Y_Isafe_silo*5Y_Isafe_silo*6Y_Isafe_silo*7Y_Isafe_silo*8Y_Isafe_silo*9Y_Iglass_shelfY_Iglass_chairY_Iglass_stoolY_Iglass_sofaY_Iglass_tableY_Iglass_table*2Y_Iglass_table*3Y_Iglass_table*4Y_Iglass_table*5Y_Iglass_table*6Y_Iglass_table*7Y_Iglass_table*8Y_Iglass_table*9Y_Iglass_chair*2Y_Iglass_chair*3Y_Iglass_chair*4Y_Iglass_chair*5Y_Iglass_chair*6Y_Iglass_chair*7Y_Iglass_chair*8Y_Iglass_chair*9Y_Iglass_sofa*2Y_Iglass_sofa*3Y_Iglass_sofa*4Y_Iglass_sofa*5Y_Iglass_sofa*6Y_Iglass_sofa*7Y_Iglass_sofa*8Y_Iglass_sofa*9Y_Iglass_stool*2Y_Iglass_stool*4Y_Iglass_stool*5Y_Iglass_stool*6Y_Iglass_stool*7Y_Iglass_stool*8Y_Iglass_stool*3Y_Iglass_stool*9Y_ICFC_100_coin_goldZvCCFC_10_coin_bronzeZvCCFC_200_moneybagZvCCFC_500_goldbarZvCCFC_50_coin_silverZvCCF_10_coin_goldZvCCF_1_coin_bronzeZvCCF_20_moneybagZvCCF_50_goldbarZvCCF_5_coin_silverZvChc_crptYmAhc_tvZ\BgothgateX~DgothiccandelabraYxBgothrailingX~Dgoth_tableYmAhc_bkshlfYmAhc_btlrY_Ihc_crtnYmAhc_djsetYmAhc_frplcZbBhc_lmpstYmAhc_machineYmAhc_rllrXQHhc_rntgnX~Dhc_trllYmAgothic_chair*1X~Dgothic_sofa*1X~Dgothic_stool*1X~Dgothic_chair*2X~Dgothic_sofa*2X~Dgothic_stool*2X~Dgothic_chair*3X~Dgothic_sofa*3X~Dgothic_stool*3X~Dgothic_chair*4X~Dgothic_sofa*4X~Dgothic_stool*4X~Dgothic_chair*5X~Dgothic_sofa*5X~Dgothic_stool*5X~Dgothic_chair*6X~Dgothic_sofa*6X~Dgothic_stool*6X~Dval_cauldronX~Dsound_machineX~Dromantique_pianochair*3Y_Iromantique_pianochair*5Y_Iromantique_pianochair*2Y_Iromantique_pianochair*4Y_Iromantique_pianochair*1Y_Iromantique_divan*3Y_Iromantique_divan*5Y_Iromantique_divan*2Y_Iromantique_divan*4Y_Iromantique_divan*1Y_Iromantique_chair*3Y_Iromantique_chair*5Y_Iromantique_chair*2Y_Iromantique_chair*4Y_Iromantique_chair*1Y_Irare_parasolY_Iplant_valentinerose*3XICplant_valentinerose*5XICplant_valentinerose*2XICplant_valentinerose*4XICplant_valentinerose*1XICplant_mazegateYeCplant_mazeZcCplant_bulrushXICpetfood4Y_Icarpet_valentineZ|Egothic_carpetXICgothic_carpet2Z|Egothic_chairX~Dgothic_sofaX~Dgothic_stoolX~Dgrand_piano*3Z|Egrand_piano*5Z|Egrand_piano*2Z|Egrand_piano*4Z|Egrand_piano*1Z|Etheatre_seatZ@Kromantique_tray2Y_Iromantique_tray1Y_Iromantique_smalltabl*3Y_Iromantique_smalltabl*5Y_Iromantique_smalltabl*2Y_Iromantique_smalltabl*4Y_Iromantique_smalltabl*1Y_Iromantique_mirrortablY_Iromantique_divider*3Z[Mromantique_divider*2Z[Mromantique_divider*4Z[Mromantique_divider*1Z[Mjp_tatami2YGGjp_tatamiYGGhabbowood_chairYGGjp_bambooYGGjp_iroriXQHjp_pillowYGGsound_set_1Y_Isound_set_2Y_Isound_set_3Y_Isound_set_4Y_Isound_set_5Z@Ksound_set_6Y_Isound_set_7Y_Isound_set_8Y_Isound_set_9Y_Isound_machine*1ZIPspotlightY_Isound_machine*2ZIPsound_machine*3ZIPsound_machine*4ZIPsound_machine*5ZIPsound_machine*6ZIPsound_machine*7ZIProm_lampZ|Erclr_sofaXQHrclr_gardenXQHrclr_chairZ|Esound_set_28Y_Isound_set_27Y_Isound_set_26Y_Isound_set_25Y_Isound_set_24Y_Isound_set_23Y_Isound_set_22Y_Isound_set_21Y_Isound_set_20Z@Ksound_set_19Z@Ksound_set_18Y_Isound_set_17Y_Isound_set_16Y_Isound_set_15Y_Isound_set_14Y_Isound_set_13Y_Isound_set_12Y_Isound_set_11Y_Isound_set_10Y_Irope_dividerXQHromantique_clockY_Irare_icecream_campaignY_Ipura_mdl5*1XQHpura_mdl5*2XQHpura_mdl5*3XQHpura_mdl5*4XQHpura_mdl5*5XQHpura_mdl5*6XQHpura_mdl5*7XQHpura_mdl5*8XQHpura_mdl5*9XQHpura_mdl4*1XQHpura_mdl4*2XQHpura_mdl4*3XQHpura_mdl4*4XQHpura_mdl4*5XQHpura_mdl4*6XQHpura_mdl4*7XQHpura_mdl4*8XQHpura_mdl4*9XQHpura_mdl3*1XQHpura_mdl3*2XQHpura_mdl3*3XQHpura_mdl3*4XQHpura_mdl3*5XQHpura_mdl3*6XQHpura_mdl3*7XQHpura_mdl3*8XQHpura_mdl3*9XQHpura_mdl2*1XQHpura_mdl2*2XQHpura_mdl2*3XQHpura_mdl2*4XQHpura_mdl2*5XQHpura_mdl2*6XQHpura_mdl2*7XQHpura_mdl2*8XQHpura_mdl2*9XQHpura_mdl1*1XQHpura_mdl1*2XQHpura_mdl1*3XQHpura_mdl1*4XQHpura_mdl1*5XQHpura_mdl1*6XQHpura_mdl1*7XQHpura_mdl1*8XQHpura_mdl1*9XQHjp_lanternXQHchair_basic*1XQHchair_basic*2XQHchair_basic*3XQHchair_basic*4XQHchair_basic*5XQHchair_basic*6XQHchair_basic*7XQHchair_basic*8XQHchair_basic*9XQHbed_budget*1XQHbed_budget*2XQHbed_budget*3XQHbed_budget*4XQHbed_budget*5XQHbed_budget*6XQHbed_budget*7XQHbed_budget*8XQHbed_budget*9XQHbed_budget_one*1XQHbed_budget_one*2XQHbed_budget_one*3XQHbed_budget_one*4XQHbed_budget_one*5XQHbed_budget_one*6XQHbed_budget_one*7XQHbed_budget_one*8XQHbed_budget_one*9XQHjp_drawerXQHtile_stellaZ[Mtile_marbleZ[Mtile_brownZ[Msummer_grill*1Y_Isummer_grill*2Y_Isummer_grill*3Y_Isummer_grill*4Y_Isummer_chair*1Y_Isummer_chair*2Y_Isummer_chair*3Y_Isummer_chair*4Y_Isummer_chair*5Y_Isummer_chair*6Y_Isummer_chair*7Y_Isummer_chair*8Y_Isummer_chair*9Y_Isound_set_36ZfIsound_set_35ZfIsound_set_34ZfIsound_set_33ZfIsound_set_32Y_Isound_set_31Y_Isound_set_30Y_Isound_set_29Y_Isound_machine_pro[~Nrare_mnstrY_Ione_way_door*1XQHone_way_door*2XQHone_way_door*3XQHone_way_door*4XQHone_way_door*5XQHone_way_door*6XQHone_way_door*7XQHone_way_door*8XQHone_way_door*9XQHexe_rugZ[Mexe_s_tableZGRsound_set_37ZfIsummer_pool*1ZlIsummer_pool*2ZlIsummer_pool*3ZlIsummer_pool*4ZlIsong_diskY_Ijukebox*1[~Ncarpet_soft_tut[~Nsound_set_44Z@Ksound_set_43Z@Ksound_set_42Z@Ksound_set_41Z@Ksound_set_40Z@Ksound_set_39Z@Ksound_set_38Z@Kgrunge_chairZ@Kgrunge_mattressZ@Kgrunge_radiatorZ@Kgrunge_shelfZ@Kgrunge_signZ@Kgrunge_tableZ@Khabboween_crypt[uKhabboween_grassZ@Khal_cauldronZ@Khal_graveZ@Ksound_set_52ZuKsound_set_51ZuKsound_set_50ZuKsound_set_49ZuKsound_set_48ZuKsound_set_47ZuKsound_set_46ZuKsound_set_45ZuKxmas_icelampZ[Mxmas_cstl_wallZ[Mxmas_cstl_twrZ[Mxmas_cstl_gate[~Ntree7Z[Mtree6Z[Msound_set_54Z[Msound_set_53Z[Msafe_silo_pb[dOplant_mazegate_snowZ[Mplant_maze_snowZ[Mchristmas_sleighZ[Mchristmas_reindeer[~Nchristmas_poopZ[Mexe_bardeskZ[Mexe_chairZ[Mexe_chair2Z[Mexe_cornerZ[Mexe_drinksZ[Mexe_sofaZ[Mexe_tableZ[Msound_set_59[~Nsound_set_58[~Nsound_set_57[~Nsound_set_56[~Nsound_set_55[~Nnoob_table*1[~Nnoob_table*2[~Nnoob_table*3[~Nnoob_table*4[~Nnoob_table*5[~Nnoob_table*6[~Nnoob_stool*1[~Nnoob_stool*2[~Nnoob_stool*3[~Nnoob_stool*4[~Nnoob_stool*5[~Nnoob_stool*6[~Nnoob_rug*1[~Nnoob_rug*2[~Nnoob_rug*3[~Nnoob_rug*4[~Nnoob_rug*5[~Nnoob_rug*6[~Nnoob_lamp*1[dOnoob_lamp*2[dOnoob_lamp*3[dOnoob_lamp*4[dOnoob_lamp*5[dOnoob_lamp*6[dOnoob_chair*1[~Nnoob_chair*2[~Nnoob_chair*3[~Nnoob_chair*4[~Nnoob_chair*5[~Nnoob_chair*6[~Nexe_globe[~Nexe_plantZ[Mval_teddy*1[dOval_teddy*2[dOval_teddy*3[dOval_teddy*4[dOval_teddy*5[dOval_teddy*6[dOval_randomizer[dOval_choco[dOteleport_door[dOsound_set_61[dOsound_set_60[dOfortune[dOsw_tableZIPsw_raven[cQsw_chestZIPsand_cstl_wallZIPsand_cstl_twrZIPsand_cstl_gateZIPgrunge_candleZIPgrunge_benchZIPgrunge_barrelZIPrclr_lampZGRprizetrophy9*1ZGRprizetrophy8*1ZGRnouvelle_traxYcPmd_rugZGRjp_tray6ZGRjp_tray5ZGRjp_tray4ZGRjp_tray3ZGRjp_tray2ZGRjp_tray1ZGRarabian_teamkZGRarabian_snakeZGRarabian_rugZGRarabian_pllwZGRarabian_divdrZGRarabian_chairZGRarabian_bigtbZGRarabian_tetblZGRarabian_tray1ZGRarabian_tray2ZGRarabian_tray3ZGRarabian_tray4ZGRPIpost.itHpost.it.vdHphotoHChessHTicTacToeHBattleShipHPokerHwallpaperHfloorHposterZ@KgothicfountainYxBhc_wall_lampZbBindustrialfanZ`BtorchZ\Bval_heartXBCwallmirrorZ|Ejp_ninjastarsXQHhabw_mirrorXQHhabbowheelZ[Mguitar_skullZ@Kguitar_vZ@Kxmas_light[~Nhrella_poster_3[Nhrella_poster_2ZIPhrella_poster_1[Nsw_swordsZIPsw_stoneZIPsw_holeZIProomdimmerZGRmd_logo_wallZGRmd_canZGRjp_sheet3ZGRjp_sheet2ZGRjp_sheet1ZGRarabian_swordsZGRarabian_wndwZGR")
                    receivedItemIndex = True
                End If

            Case "B^" '// User modifies something with badge
                Dim badgeLen As Integer = HoloENCODING.decodeB64(currentPacket.Substring(2, 2))
                Dim badgeEnabled As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(badgeLen + 4, 1))
                Dim pBadge As String = currentPacket.Substring(4, badgeLen)

                Dim myBadges() As String = HoloDB.runReadArray("SELECT badgeid FROM users_badges WHERE userid = '" & UserID & "'", True)

                For i = 0 To myBadges.Count - 1
                    If myBadges(i) = pBadge Then Exit For '// Badge found, stop searching
                    If i = myBadges.Count - 1 Then If Not (myBadges(i) = pBadge) Then Return '// We are at last badge in the badges the user owns, and still not found, the user does not have this badge!
                Next

                Dim updatePacket As String = "Cd" & HoloENCODING.encodeVL64(userDetails.roomUID)
                If badgeEnabled = 1 Then
                    userDetails.nowBadge = pBadge
                    updatePacket += pBadge
                Else
                    userDetails.nowBadge = vbNullString
                End If

                roomCommunicator.sendAll(updatePacket & sysChar(1))
                HoloDB.runQuery("UPDATE users SET badgestatus = '" & badgeEnabled & "," & pBadge & "' WHERE id = '" & UserID & "' LIMIT 1")

            Case "@t", "@x", "@w" '// User speaks/shouts/whispers
                Room_Talk()

            Case "D}" '// Show 'talking...' speech bubble
                roomCommunicator.sendAll("Ei" & HoloENCODING.encodeVL64(userDetails.roomUID) & "I" & sysChar(1))

            Case "D~" '// Hide 'talking...' speech bubble
                roomCommunicator.sendAll("Ei" & HoloENCODING.encodeVL64(userDetails.roomUID) & "H" & sysChar(1))

            Case "A^" '// User waves (bad habit =[)
                userDetails.Wave()

            Case "A]" '// User dances
                If currentPacket.Length = 2 Then userDetails.Dance(0) Else userDetails.Dance(HoloENCODING.decodeVL64(currentPacket.Substring(2)))

            Case "Ah" '// User votes in the Lido
                userDetails.showLidoVote(safeParse(currentPacket.Substring(2)))

            Case "As" '// User clicks door of the room
                Room_noRoom(True, True)

            Case "AX" '// Stop an action/status
                Dim toStop As String = currentPacket.Substring(2)
                If toStop = "CarryItem" Then
                    userDetails.removeStatus("drink")
                    userDetails.removeStatus("carryd")
                ElseIf toStop = "Dance" Then
                    userDetails.removeStatus("dance")
                End If

                Try
                    roomCommunicator.refreshUser(userDetails)
                Catch
                End Try

            Case "AP" '// Start carrying an item
                userDetails.CarryItem(currentPacket.Substring(2))

            Case "AK" '// Walking
                userDetails.DestX = HoloENCODING.decodeB64(currentPacket.Substring(2, 2))
                userDetails.DestY = HoloENCODING.decodeB64(currentPacket.Substring(4, 2))

            Case "Bk" '// Game walking/actioning [BB/SS]
                Dim coX As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim coY As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(HoloENCODING.encodeVL64(coX).Length + 2))

            Case "DG" '// Tags?
                transData("DuH" & sysChar(1))
                'transData("E^" & HoloENCODING.encodeVL64(UserID) & HoloENCODING.encodeVL64(2) & "Tag1" & sysChar(2) & "Tag2" & sysChar(2) & sysChar(1))

                '// Game

            Case "Ai" '// Buy tickets
                Dim ticketAmount As Integer = 2
                Dim Price As Integer = 1
                Dim forUser As String = currentPacket.Substring(5).ToLower
                Dim nowCredits As Integer = HoloDB.runRead("SELECT credits FROM users WHERE id = '" & UserID & "' LIMIT 1")

                If currentPacket.Substring(2, 1) = "J" Then ticketAmount = 20 : Price = 6
                If Price > nowCredits Then transData("AD" & sysChar(1)) : Return
                nowCredits -= Price

                If Not (forUser) = userDetails.Name.ToLower Then If HoloDB.checkExists("SELECT id FROM users WHERE name = '" & forUser & "' LIMIT 1") = False Then transData("AL" & forUser & sysChar(1)) : Return

                HoloDB.runQuery("UPDATE users SET credits = '" & nowCredits & "' WHERE id = '" & UserID & "' LIMIT 1")

                If forUser = userDetails.Name.ToLower Then
                    HoloDB.runQuery("UPDATE users SET tickets = tickets + " & ticketAmount & " WHERE id = '" & UserID & "' LIMIT 1")
                    transData("A|" & HoloDB.runRead("SELECT tickets FROM users WHERE id = '" & UserID & "' LIMIT 1") & sysChar(1))
                Else
                    HoloDB.runQuery("UPDATE users SET tickets = tickets + " & ticketAmount & " WHERE name = '" & forUser & "' LIMIT 1")
                    Dim receiverID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & forUser & "' LIMIT 1")
                    If HoloMANAGERS.isOnline(receiverID) Then HoloMANAGERS.getUserClass(receiverID).transData("A|" & HoloDB.runRead("SELECT tickets FROM users WHERE id = '" & receiverID & "' LIMIT 1") & sysChar(1))
                End If

                transData("@F" & nowCredits & ".0" & sysChar(1))

            Case "B_" '// Game - New Ch packet (current game list) request
                If userDetails.inBBLobby = True Then transData(HoloBBGAMELOBBY.getGameList)

            Case "Bb" '// Game - create game request
                If userDetails.inBBLobby = True Then
                    transData("CkPDfieldTypeHJIIIIQAmaximumSimultaneousPowerupsHIKIIIRBpowerupCreateChanceHIRLIIIPYnumTeamsHJJIJIPAcoloringForOpponentTimePulsesHISCIIIPYgameLengthSecondsHIPmIHHallowedPowerupsIJ" & HoloBBGAMELOBBY.allowedPowerUps & ",9HcleaningTilesTimePulsesHISCIIIPYpowerupCreateFirstTimePulsesHIHIHIPYsecondsUntilRestartHIRGIHHpowerupTimeToLivePulsesHIPOIQAIPYpowerupCreateIntervalPulsesHIPEIQAIPYstunTimePulsesHIRBIIIPYhighJumpsTimePulsesHISCIIIPYnameIJHsecondsUntilStartHISCIHH")
                End If

            Case "Bc" '// Game - created game
                If userDetails.inBBLobby = True Then HoloBBGAMELOBBY.createGame(userDetails, currentPacket)

            Case "B`" '// Game - enter game sub
                If userDetails.inBBLobby = True And userDetails.Game_withState = -1 Then DirectCast(HoloBBGAMELOBBY.Games(HoloENCODING.decodeVL64(currentPacket.Substring(2))), clsHoloBBGAME).modViewers(userDetails, False)

            Case "Bg" '// Game - leave sub
                If userDetails.inBBLobby = True Then
                    If userDetails.Game_owns = True Then
                        HoloBBGAMELOBBY.destroyGame(userDetails.Game_ID)
                    Else
                        Dim gameInstance As clsHoloBBGAME = HoloBBGAMELOBBY.Games(userDetails.Game_ID)

                        If userDetails.Game_withState >= 0 Then '// User is in a team; .Game_withState is the team ID
                            gameInstance.modTeam(userDetails, userDetails.Game_withState, -1)
                        ElseIf userDetails.Game_withState = -2 Then '// User is just viewing the sub
                            gameInstance.modViewers(userDetails, False, True)
                        ElseIf userDetails.Game_withState = -3 Then '// User is a spectator
                            gameInstance.modViewers(userDetails, True, True)
                        End If

                        transData("CmH" & sysChar(1))
                    End If
                End If

            Case "Be" '// Game - join/leave teams
                If userDetails.inBBLobby = True And Not (userDetails.Game_withState = -1) Then
                    If Integer.Parse(HoloDB.runRead("SELECT tickets FROM users WHERE id = '" & UserID & "' LIMIT 1")) < 2 Then
                        transData("ClJ" & sysChar(1))
                    Else
                        Dim CBA As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                        Dim newTeamID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(HoloENCODING.encodeVL64(CBA).Length + 2))

                        DirectCast(HoloBBGAMELOBBY.Games(userDetails.Game_ID), clsHoloBBGAME).modTeam(userDetails, userDetails.Game_withState, newTeamID)
                    End If
                End If

            Case "Bj" '// Game - attempt to start game
                DirectCast(HoloBBGAMELOBBY.Games(userDetails.Game_ID), clsHoloBBGAME).startGame()

            Case "Ae" '// User opens catalogue, send pages index
                Dim listBuilder As New StringBuilder("A~")
                Dim pageNames() As String = HoloDB.runReadArray("SELECT indexname FROM catalogue_pages WHERE minrank <= '" & userDetails.Rank & "' ORDER BY indexid ASC", True)
                For i = 0 To pageNames.Count - 1
                    If HoloRACK.cataloguePages.ContainsKey(pageNames(i)) Then listBuilder.Append(pageNames(i) & sysChar(9) & DirectCast(HoloRACK.cataloguePages(pageNames(i)), clsHoloRACK.cachedCataloguePage).displayName & sysChar(13))
                Next
                listBuilder.Append(sysChar(1))
                transData(listBuilder.ToString)

            Case "Af" '// User opens catalogue page
                Dim pageIndexName As String = currentPacket.Split("/")(1)
                If HoloRACK.cataloguePages.ContainsKey(pageIndexName) = False Then Return
                Try
                    transData("A" & DirectCast(HoloRACK.cataloguePages(pageIndexName), clsHoloRACK.cachedCataloguePage).strPage & sysChar(1))
                Catch
                    errorUser()
                End Try


            Case "AA" '// User uses the Hand (not to bring a nazi greet, but you know...)
                refreshHand(currentPacket.Substring(2))

            Case "Ad" '// User buys something out of the catalogue
                Dim packetContent() As String = currentPacket.Split(sysChar(13))
                Dim fromPage As String = packetContent(1)
                Dim wantedItem As String = packetContent(3)

                Try
                    Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM catalogue_items WHERE name_cct = '" & wantedItem & "' LIMIT 1")
                    Dim pageID As Integer = HoloDB.runRead("SELECT indexid FROM catalogue_pages WHERE indexname = '" & fromPage & "' AND minrank <= '" & userDetails.Rank & "' LIMIT 1")
                    Dim itemCost As Integer = HoloDB.runRead("SELECT catalogue_cost FROM catalogue_items WHERE tid = '" & templateID & "' LIMIT 1")
                    Dim myCredits As Integer = HoloDB.runRead("SELECT credits FROM users WHERE id = '" & UserID & "' LIMIT 1")

                    If itemCost > myCredits Then
                        transData("AD" & sysChar(1))
                    Else
                        myCredits -= itemCost
                        transData("@F" & myCredits & ".0" & sysChar(1))
                        HoloDB.runQuery("INSERT INTO furniture(tid,roomid,inhand,x,y,z,h) VALUES ('" & templateID & "','0','" & UserID & "','0','0','0','0')")
                        HoloDB.runQuery("UPDATE users SET credits = '" & myCredits & "' WHERE id = '" & UserID & "' LIMIT 1")
                        refreshHand("last")

                        If HoloITEM(templateID).cctName = "roomdimmer" Then
                            Dim itemID As Integer = HoloDB.runRead("SELECT id FROM furniture WHERE inhand = '" & UserID & "' ORDER BY id DESC LIMIT 1") '// Get the ID of the currently bought furniture, by getting the heighest ID furniture in the users hand
                            Dim defaultPreset As String = "1,#000000,155" '// Edit to your wishes
                            Dim defaultSetPreset As String = "1,1,1,#000000,155" '// Edit to your wishes
                            HoloDB.runQuery("INSERT INTO furniture_moodlight(id,roomid,preset_cur,preset_1,preset_2,preset_3) VALUES ('" & itemID & "','0','1','" & defaultPreset & "','" & defaultPreset & "','" & defaultPreset & "')")
                            HoloDB.runQuery("UPDATE furniture SET opt_var = '" & defaultSetPreset & "' WHERE id = '" & itemID & "' LIMIT 1")
                        End If
                    End If

                Catch '// Page not found, user doesn't has access to page or something went wrong!
                    errorUser()
                    MsgBox(Err.Description)
                End Try

            Case "AZ" '// User places item down
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.isOwner = False Then Return
                roomCommunicator.placeItem(UserID, currentPacket.Substring(2))

            Case "AC" '// User picks item up
                '// ACnew stuff 10
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.isOwner = False Then Return
                roomCommunicator.removeItem(UserID, Integer.Parse(currentPacket.Split(" ")(2)))
                refreshHand("last")

            Case "AI" '// User rotates/moves item
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.isOwner = False Then Return

                Dim packetContent() As String = currentPacket.Substring(2).Split(" ")
                roomCommunicator.relocateItem(packetContent(0), packetContent(1), packetContent(2), packetContent(3))


            Case "Cm" '// User wants to send a CFH message
                Dim cfhStats() As String = HoloDB.runReadArray("SELECT id,date,message FROM cms_help WHERE username = '" & userDetails.Name & "' LIMIT 1")
                If cfhStats.Count = 0 Then
                    transData("D" & "H" & sysChar(1))
                Else
                    transData("D" & "I" & cfhStats(0) & sysChar(2) & cfhStats(1) & sysChar(2) & cfhStats(2) & sysChar(2) & sysChar(1))
                End If

            Case "Cn" '// User deletes his pending CFH message
                HoloDB.runQuery("DELETE FROM cms_help WHERE username = '" & userDetails.Name & "' LIMIT 1")
                transData("DH" & sysChar(1))

            Case "AV" '// User sends CFH message
                If HoloDB.checkExists("SELECT id FROM cms_help WHERE username = '" & userDetails.Name & "' LIMIT 1") = True Then Return
                Dim messageLength As Integer = HoloENCODING.decodeB64(currentPacket.Substring(2, 2))
                Dim cfhMessage As String = currentPacket.Substring(4, messageLength)
                If cfhMessage.Length = 0 Then Return

                HoloDB.runQuery("INSERT INTO cms_help (username,ip,message,date,picked_up,subject,roomid) VALUES ('" & userDetails.Name & "','" & userSocket.RemoteEndPoint.ToString.Split(":")(0) & "','" & HoloDB.fixChars(cfhMessage) & "','" & DateTime.Now & "','0','CFH message [Hotel]','" & userDetails.roomID & "')")
                Dim cfhID As Integer = HoloDB.runRead("SELECT id FROM cms_help WHERE username = '" & userDetails.Name & "' LIMIT 1")
                Dim roomName As String
                If userDetails.inPublicroom = True Then
                    roomName = HoloDB.runRead("SELECT name_caption FROM publicrooms WHERE id = '" & userDetails.roomID & "' LIMIT 1")
                Else
                    roomName = HoloDB.runRead("SELECT name FROM guestrooms WHERE id = '" & userDetails.roomID & "' LIMIT 1")
                End If

                transData("EAH" & sysChar(1))
                HoloMANAGERS.sendToRank(6, True, "BT" & HoloENCODING.encodeVL64(cfhID) & sysChar(2) & "I" & DateTime.Now.ToString & sysChar(2) & userDetails.Name & sysChar(2) & cfhMessage & sysChar(2) & HoloENCODING.encodeVL64(userDetails.roomID) & sysChar(2) & roomName & sysChar(2) & "I" & sysChar(2) & HoloENCODING.encodeVL64(userDetails.roomID) & sysChar(1))

            Case "CG" ' // CFH center - reply call
                If HoloRANK(userDetails.Rank).containsRight("fuse_receive_calls_for_help") = False Then Return
                Dim cfhID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2))))
                Dim cfhReply As String = currentPacket.Substring(cfhID.ToString.Length + 6)

                Dim toUserName As String = HoloDB.runRead("SELECT username FROM cms_help WHERE id = '" & cfhID & "' LIMIT 1")
                If toUserName = vbNullString Then
                    transData("BK" & "Call already handled by you/other Staff members and flagged as 'completed'." & sysChar(1))
                Else
                    Dim toUserID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & toUserName & "' LIMIT 1")
                    If HoloMANAGERS.isOnline(toUserID) = True Then HoloMANAGERS.getUserClass(toUserID).transData("DR" & cfhReply & sysChar(2) & sysChar(1))
                End If

            Case "CF" '// CFH center - 'release' call, other MOD's will see it and the first one who assings it (@p) to itself, handles it
                If HoloRANK(userDetails.Rank).containsRight("fuse_receive_calls_for_help") = False Then Return
                Dim cfhID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(4))
                Dim cfhStats() As String = HoloDB.runReadArray("SELECT username,message,date,roomid FROM cms_help WHERE id = '" & cfhID & "' LIMIT 1")
                If cfhStats.Count = 0 Then
                    transData("BK" & "Call [" & cfhID & "] handled by you/other Staff members and flagged as 'completed'." & sysChar(1))
                Else
                    HoloMANAGERS.sendToRank(6, True, "BT" & HoloENCODING.encodeVL64(cfhID) & sysChar(2) & "I" & cfhStats(2) & sysChar(2) & cfhStats(0) & sysChar(2) & cfhStats(1) & sysChar(2) & HoloENCODING.encodeVL64(cfhStats(3)) & sysChar(2) & "-" & sysChar(2) & "I" & sysChar(2) & HoloENCODING.encodeVL64(cfhStats(3)) & sysChar(1))
                End If

            Case "@p" '// CFH center - assign call to yourself, you'll sort it out
                If HoloRANK(userDetails.Rank).containsRight("fuse_receive_calls_for_help") = False Then Return
                Dim cfhID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(4))
                If HoloDB.checkExists("SELECT id FROM cms_help WHERE id = '" & cfhID & "' LIMIT 1") = False Then
                    transData("BK" & "Call [" & cfhID & "] already handled by you/other Staff members and flagged as 'completed'." & sysChar(1))
                Else
                    HoloDB.runQuery("UPDATE cms_help SET picked_up = '" & userDetails.Name & "' WHERE id = '" & cfhID & "' LIMIT 1")
                    ' HoloDB.runQuery("DELETE FROM cms_help WHERE id = '" & cfhID & "' LIMIT 1")
                    ' transData("BK" & "Call [" & cfhID & "] succesfully dropped." & sysChar(1))
                End If

            Case "A`" '// Give rights to someone in your guestroom, you need to be roomowner
                If userDetails.isOwner = False Then Return
                Dim toUser As String = currentPacket.Substring(2)
                roomCommunicator.modRights(toUser, True)

            Case "Aa" '// Remove rights from someone in your guestroom, you need to be roomowner
                If userDetails.isOwner = False Then Return
                Dim toUser As String = currentPacket.Substring(2)
                roomCommunicator.modRights(toUser, False)

            Case "A_" '// Kick a user from guestoom, must have rights, or be roomowner/staff
                If userDetails.hasRights = False Then Return
                Dim kickTarget As String = currentPacket.Substring(2)
                roomCommunicator.kickUser(kickTarget, userDetails.Rank)

            Case "Cw" '// User spins Habbowheel/wheel of fortune
                If userDetails.hasRights = False Then Return
                Dim itemID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                roomCommunicator.spinHabbowheel(itemID)

            Case "EU" '// Load moodlight settings
                If userDetails.isOwner = False Then Return '// Only room owners [includes staff] are allowed to adjust the moodlight, so if they aren't owner then they don't need the settings!
                Dim settingData As String = roomCommunicator.moodLight_GetSettings
                If Not (settingData = vbNullString) Then transData("Em" & settingData & sysChar(1))

            Case "EV" '// Apply modified moodlight settings
                If userDetails.isOwner = False Then Return '// Only room owners [includes staff] are allowed to adjust the moodlight
                Dim presetID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1))
                Dim bgState As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3, 1))
                Dim presetColour As String = currentPacket.Substring(6, HoloENCODING.decodeB64(currentPacket.Substring(4, 2)))
                Dim presetDarkF As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(presetColour.Length + 6))
                roomCommunicator.moodLight_SetSettings(True, presetID, bgState, presetColour, presetDarkF)

            Case "EW" '// Turn moodlight on/off
                If userDetails.isOwner = False Then Return
                roomCommunicator.moodLight_SetSettings(False, 0, 0, vbNullString, 0)


        End Select
        'Catch ex As Exception
        'Console.WriteLine("[ERROR] " & ex.Message & " at packet " & currentPacket & ".")

        'End Try
    End Sub
    Friend Sub killConnection(Optional ByRef debugMessage As String = vbNullString)
        If killedConnection = True Then Return '// Already killed connnection

        On Error Resume Next '// Just let it handle this procedure, we're not interested in any error catching or w/e
        userSocket.Close(1) '// Wait a second and then close the socket

        If HoloMANAGERS.hookedUsers.ContainsKey(UserID) = True Then HoloMANAGERS.hookedUsers.Remove(Me) '// Remove this user class from the hookedUsers
        If pingManager.IsAlive = True Then pingManager.Abort() '// If the userpinger is running (obv in most cases) then abort it
        If userDetails.roomID > 0 Then Room_noRoom(True, False)
        HoloSCKMGR.flagSocketAsFree(classID) '// Flag this socket as free again for the socket manager
        userDetails = Nothing

        killedConnection = True
        Me.Finalize() '// Destroy this class and make it available to .NET's garabage collector
        Console.WriteLine("[SCKMGR] [" & classID & "] dumped and all familar resources destroyed.") '// Print that the clearup has succeeded
        If Not (debugMessage = vbNullString) Then Console.WriteLine("[SCKMGR] Reason: " & debugMessage)
    End Sub
    Friend Sub Refresh()
        If IsNothing(roomCommunicator) = False Then roomCommunicator.refreshUser(userDetails)
    End Sub
#End Region
#Region "Client actions - all action subs"
#Region "Badges, club & SSO actions"
    Private Sub processBadges()
        Dim b, activeBadgeSlot As Integer
        Dim myBadges(), myBadgeStatus() As String

        myBadges = HoloDB.runReadArray("SELECT badgeid FROM users_badges WHERE userid = '" & UserID & "'", True)

        If myBadges.Count > 0 Then '// If this user has badges
            Dim badgePacketBuilder As New StringBuilder("Ce" & HoloENCODING.encodeVL64(myBadges.Count))
            myBadgeStatus = HoloDB.runRead("SELECT badgestatus FROM users WHERE id = '" & UserID & "'").Split(",")

            For b = 0 To myBadges.Count - 1
                badgePacketBuilder.Append(myBadges(b) & sysChar(2))
                If activeBadgeSlot = 0 Then If myBadges(b) = myBadgeStatus(1) Then activeBadgeSlot = b
            Next

            If Integer.Parse(myBadgeStatus(0)) = 1 Then userDetails.nowBadge = myBadgeStatus(1)
            transData(badgePacketBuilder.ToString & HoloENCODING.encodeVL64(activeBadgeSlot) & HoloENCODING.encodeVL64(myBadgeStatus(0)) & sysChar(1))
        Else
            transData("CeHH" & sysChar(1))
        End If
    End Sub
    Private Sub processConsole()
        Dim consolePack As New StringBuilder("@L") '// Create a stringbuilder for the Console packet, so we don't have to wrestle the bigger and bigger growing packet through the memory every time. Make the packet start with @L (packet header)

        '// Add the users Console mission to the packet, followed by a Char 2
        consolePack.Append(userDetails.consoleMission & sysChar(2))

        '// Add some number encoding
        consolePack.Append(HoloENCODING.encodeVL64(600) & HoloENCODING.encodeVL64(200) & HoloENCODING.encodeVL64(600))

        '// Get the users friends (using one row, yay, if someone knows a better way (so you don't get the friendid's equal to your userid or vice versa) please help!
        Dim curFriendID As Integer
        Dim friendList(), friendLink(), friendDetails() As String

        friendList = HoloDB.runReadArray("SELECT userid,friendid FROM messenger_friendships WHERE userid = '" & UserID & "' OR friendid = '" & UserID & "'", True)
        If friendList.Count = 0 Then consolePack.Append(HoloENCODING.encodeVL64(200)) Else consolePack.Append(HoloENCODING.encodeVL64(friendList.Count))

        For curRowID = 0 To friendList.Count - 1 '// Go along all found friends
            friendLink = friendList(curRowID).Split(sysChar(9))
            If Integer.Parse(friendLink(0)) = UserID Then curFriendID = Integer.Parse(friendLink(1)) Else curFriendID = Integer.Parse(friendLink(0))
            friendDetails = HoloDB.runReadArray("SELECT name,figure,consolemission,lastvisit FROM users WHERE id = '" & curFriendID & "' LIMIT 1")
            If friendDetails.Count = 0 Then '// This friend is dead or does not exist! :D Delete the friendship relation between UserID and curFriendID, since we use one row per friendship it checks two times
                HoloDB.runRead("DELETE FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & curFriendID & "') OR (userid = '" & curFriendID & "' AND friendid = '" & UserID & "') LIMIT 1")
            Else '// Friend was found, add his details to the packet
                Dim userPosition As String = HoloMANAGERS.getUserHotelPosition(curFriendID)
                If userPosition = "H" Then userPosition += sysChar(2) & friendDetails(3) Else userPosition += sysChar(2) & DateTime.Now.ToString
                If friendDetails(2) = "" Then friendDetails(2) = "H" Else friendDetails(2) = "I" & friendDetails(2)

                consolePack.Append(HoloENCODING.encodeVL64(curFriendID)) '// Encode friend's user ID and add it
                consolePack.Append(friendDetails(0)) '// Add friend's name
                consolePack.Append(sysChar(2))
                consolePack.Append(friendDetails(2)) '// Add friend's consolemission
                consolePack.Append(sysChar(2))
                consolePack.Append(userPosition) '// Add friend's Hotel position/lastvisit
                consolePack.Append(sysChar(2))
                consolePack.Append(friendDetails(1)) '// Add friend's figure/look
                consolePack.Append(sysChar(2))
            End If
        Next '// Next friend and repeat the procedure till all friends are added

        consolePack.Append(sysChar(1)) '// Close the packet for consolemission + friends

        transData(consolePack.ToString) '// Send the consolemission + friends

        '// Get the users pending friend requests
        consolePack = New StringBuilder '// Reset the console pack stringbuilder
        Dim requestList() As String = HoloDB.runReadArray("SELECT userid_from FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' ORDER BY requestid ASC", True)
        For curRequester = 0 To requestList.Count - 1 '// Go along all found requesters
            consolePack.Append("BD") '// Add header
            consolePack.Append(HoloENCODING.encodeVL64(requestList(curRequester))) '// Add encoded requester's userid
            consolePack.Append(HoloDB.runRead("SELECT name FROM users WHERE id = '" & requestList(curRequester) & "' LIMIT 1")) '// Read requester's name from database and add
            consolePack.Append(sysChar(2))
            consolePack.Append(sysChar(1))
        Next '// Next friend request and repeat the procedure till all friends are added

        transData(consolePack.ToString) '// Send the friend requests

        '// Get the users new messages
        consolePack = New StringBuilder '// Reset the console pack stringbuilder
        '// Split the four fields dvirectly, if we did it with one query for all messages it would become a BIG file which will make the server wrestle around with it, thought about that when I saw Jeax's JASE ;]
        Dim messageFriendIDs() As String = HoloDB.runReadArray("SELECT friendid FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid DESC", True)
        Dim messageIDs() As String = HoloDB.runReadArray("SELECT messageid FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid DESC", True)
        Dim messageDates() As String = HoloDB.runReadArray("SELECT sent_on FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid DESC", True)
        Dim messageTexts() As String = HoloDB.runReadArray("SELECT message FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid DESC", True)

        For curMessage = 0 To messageFriendIDs.Count - 1 '// Go along all new messages
            consolePack.Append("BF") '// Add header
            consolePack.Append(HoloENCODING.encodeVL64(messageIDs(curMessage))) '// Add encoded message ID of this message
            consolePack.Append(HoloENCODING.encodeVL64(messageFriendIDs(curMessage))) '// Add encoded sender user ID of this message
            consolePack.Append(messageDates(curMessage)) '// Add the datestamp of this message
            consolePack.Append(sysChar(2))
            consolePack.Append(messageTexts(curMessage)) '// Add the text of this message
            consolePack.Append(sysChar(2))
            consolePack.Append(sysChar(1))
        Next

        transData(consolePack.ToString) '// Send the messages
    End Sub
    Private Sub processClub()
        '// Thanks to Jeax for examples in JASE, I am bad at doing stuff with dates/months w/e
        Dim restingDays, passedMonths, restingMonths As Integer
        Dim dbRow() As String

        dbRow = HoloDB.runReadArray("SELECT months_expired,months_left,date_monthstarted FROM users_club WHERE userid = '" & UserID & "'")
        If dbRow.Count > 0 Then '// If the user has subscribed to Club
            passedMonths = Integer.Parse(dbRow(0)) '// Get the amount of expired months
            restingMonths = Integer.Parse(dbRow(1)) - 1 '// Get the amount of resting months and manipulate this count just for correct packet
            restingDays = (DateTime.Parse(dbRow(2))).Subtract(DateTime.Now).TotalDays + 31 '// Get the amount of resting days
            If userDetails.Rank = 1 Then '// If user is normal user and club, but hasn't received the club rank
                HoloDB.runQuery("UPDATE users SET rank = '2' WHERE id = '" & UserID & "' LIMIT 1") '// Update rank in database
                userDetails.Rank = 2
            End If
            userDetails.clubMember = True
        End If

        transData("@Gclub_habbo" & sysChar(2) & HoloENCODING.encodeVL64(restingDays) & HoloENCODING.encodeVL64(passedMonths) & HoloENCODING.encodeVL64(restingMonths) & "I" & sysChar(1))
    End Sub
    Friend Sub refreshAppearance(ByVal reloadFromDB As Boolean)
        If reloadFromDB = True Then
            Dim userData() As String = HoloDB.runReadArray("SELECT figure,sex,mission FROM users WHERE id = '" & UserID & "' LIMIT 1")
            userDetails.Figure = userData(0)
            userDetails.Sex = Char.Parse(userData(1))
            userDetails.Mission = userData(2)
        End If
        transData("@E" & classID & sysChar(2) & userDetails.Name & sysChar(2) & userDetails.Figure & sysChar(2) & userDetails.Sex & sysChar(2) & userDetails.Mission & sysChar(2) & "Hch=s02/253,146,160" & sysChar(2) & "HI" & sysChar(1))
        If IsNothing(roomCommunicator) = False Then roomCommunicator.sendAll("DJ" & HoloENCODING.encodeVL64(userDetails.roomUID) & userDetails.Figure & sysChar(2) & userDetails.Sex & sysChar(2) & userDetails.Mission & sysChar(2) & sysChar(1)) '// Poof and refresh users look in room [only if the user is in room, thus the roomCommunicator is not nulled]
    End Sub
    Friend Sub refreshValuables()
        Dim userData() As String = HoloDB.runReadArray("SELECT credits,tickets FROM users WHERE id = '" & UserID & "' LIMIT 1")
        transData("@F" & userData(0) & sysChar(1) & "A|" & userData(1) & sysChar(1))
    End Sub
    Friend Sub handleBan(ByVal strMessage As String)
        Try
            transData("@c" & strMessage & sysChar(1))
            killConnection("Banned [" & strMessage & "]")
        Catch
        End Try
    End Sub
#End Region
#Region "Console (messenger) actions"
    Private Sub Console_FriendRequest()
        Dim requesterName As String = currentPacket.Substring(4)
        Dim requesterID = HoloDB.runRead("SELECT id FROM users WHERE name = '" & requesterName & "' LIMIT 1")
        If requesterID = vbNullString Then Return '// Non-existring user
        requesterID = Integer.Parse(requesterID)

        If HoloDB.checkExists("SELECT requestid FROM messenger_friendrequests WHERE userid_to = '" & requesterID & "' AND userid_from = '" & UserID & "' LIMIT 1") = True Then Return '// Already a pending request for this friendship
        Dim requestID As Integer = Val(HoloDB.runRead("SELECT MAX(requestid) FROM messenger_friendrequests WHERE userid_to = '" & requesterID & "'")) + 1 '// Get the next requestid in line (so they appear in correct order at login ;])
        '// Insert the friendrequest in the table
        HoloDB.runQuery("INSERT INTO messenger_friendrequests(userid_to,userid_from,requestid) VALUES ('" & requesterID & "','" & UserID & "','" & requestID & "')")

        '// If the requested one is online, then send the 'you got new friendrequest' message
        If HoloMANAGERS.isOnline(requesterID) = True Then HoloMANAGERS.getUserClass(requesterID).transData("BD" & HoloENCODING.encodeVL64(UserID) & userDetails.Name & sysChar(2) & sysChar(1))
    End Sub
    Private Sub Console_FriendAccept()
        Dim cntAcceptedRequests As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2)) '// Get the count of accepted requests (yes decoding whole string will give us the value of the first encoded in row ;]
        Dim requesterIDs(200) As Integer '// Create a static array with max member count 200 so users can't accept more than 200 users at a time (popular much?)

        currentPacket = currentPacket.Substring(2 + HoloENCODING.encodeVL64(cntAcceptedRequests).Length) '// Get the length of the encoded count of accepted ones, and cut it off the packet together with the header, so we don't include that part in our encoding/decoding cycle all the time
        For x = 0 To cntAcceptedRequests - 1 '// Start a loop to get all IDs the user wants to accept
            If currentPacket.Length = 0 Then Return '// Probably a junior scripter tried to fook the encoding, quit! (if you want to be mean put killConnection() there ;])
            requesterIDs(x) = HoloENCODING.decodeVL64(currentPacket) '// Get the next ID in row by decoding the remaining packet (yes, decoding the whole row will give us the value of the first encoded ID in row ;])
            currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(requesterIDs(x)).Length) '// Encode the found ID which you just decoded, to and check the length of it's encoded variant, and cut that piece off the remaining packet
        Next '// Attempt to get the next one

        '// Users own details (create a pre-made packet ;D)
        Dim myImAddedPack As String = "BI" & HoloENCODING.encodeVL64(UserID) & userDetails.Name & sysChar(2) & "I" & HoloDB.runRead("SELECT consolemission FROM users WHERE id = '" & UserID & "' LIMIT 1") & sysChar(2) & HoloMANAGERS.getUserHotelPosition(UserID) & sysChar(2) & DateTime.Now.ToString & sysChar(2) & userDetails.Figure & sysChar(2) & sysChar(1)
        Dim myCompletionPack As New StringBuilder '// Pack for the user itself with the new content for it's friendlist =]

        For x = 0 To cntAcceptedRequests - 1 '// Process all accepted ones
            If Not (HoloDB.runRead("SELECT requestid FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' AND userid_from = '" & requesterIDs(x) & "' LIMIT 1") = vbNullString) Then
                HoloDB.runQuery("INSERT INTO messenger_friendships(userid,friendid) VALUES ('" & requesterIDs(x) & "','" & UserID & "')") '// If there's really a friend request between the user and this requester, then insert a new friendrow in the messenger_friendships table
                HoloDB.runQuery("DELETE FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' AND userid_from = '" & requesterIDs(x) & "'") '// Delete all pending requests between these users (sometimes there appear multiple ones) 
            End If

            myCompletionPack.Append("BI" & HoloENCODING.encodeVL64(requesterIDs(x)))
            If HoloMANAGERS.isOnline(requesterIDs(x)) = True Then
                Dim requesterDetails As clsHoloUSERDETAILS = HoloMANAGERS.getUserDetails(requesterIDs(x))
                myCompletionPack.Append(requesterDetails.Name & sysChar(2) & "I" & requesterDetails.consoleMission & sysChar(2) & HoloMANAGERS.getUserHotelPosition(requesterIDs(x)) & sysChar(2) & DateTime.Now.ToString & sysChar(2) & requesterDetails.Figure & sysChar(2) & sysChar(1))
                requesterDetails.userClass.transData(myImAddedPack)
            Else
                Dim requesterDetails() As String = HoloDB.runReadArray("SELECT name,figure,consolemission,lastvisit FROM users WHERE id = '" & requesterIDs(x) & "' LIMIT 1")
                myCompletionPack.Append(requesterDetails(0) & sysChar(2) & "I" & requesterDetails(2) & sysChar(2) & "H" & sysChar(2) & requesterDetails(3) & sysChar(2) & requesterDetails(1) & sysChar(2) & sysChar(1))
            End If

            myCompletionPack.Append("D{" & sysChar(2) & "H" & sysChar(1))
        Next x

        transData(myCompletionPack.ToString)
    End Sub
    Private Sub Console_FriendDecline()
        Dim cntDeclinedRequests As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2)) '// Get the count of declined requests (yes decoding whole string will give us the value of the first encoded in row ;]
        Dim declinerIDs(200) As Integer '// Create a static array with max member count 200 so users can't decline more than 200 users at a time (popular much?)

        currentPacket = currentPacket.Substring(2 + HoloENCODING.encodeVL64(cntDeclinedRequests).Length) '// Get the length of the encoded count of declined ones, and cut it off the packet together with the header, so we don't include that part in our encoding/decoding cycle all the time
        For x = 0 To cntDeclinedRequests - 1 '// Start a loop to get all IDs the user wants to decline
            If currentPacket.Length = 0 Then Return '// Probably a junior scripter tried to fook the encoding, quit! (if you want to be mean put killConnection() there ;])
            declinerIDs(x) = HoloENCODING.decodeVL64(currentPacket) '// Get the next ID in row by decoding the remaining packet (yes, decoding the whole row will give us the value of the first encoded ID in row ;])
            currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(declinerIDs(x)).Length) '// Encode the found ID which you just decoded, to and check the length of it's encoded variant, and cut that piece off the remaining packet
        Next '// Attempt to get the next one

        For x = 0 To cntDeclinedRequests - 1 '// Attempt to delete declined requests between this user and his requesters
            HoloDB.runQuery("DELETE FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' AND userid_from = '" & declinerIDs(x) & "' LIMIT 1")
        Next
    End Sub
    Private Sub Console_FriendDelete()
        Dim toDeleteID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get the ID of the friend the user wants to delete
        transData("BJI" & HoloENCODING.encodeVL64(toDeleteID) & sysChar(1)) '// Send a packet to the user which removes the friend from his list in the console
        If HoloDB.checkExists("SELECT userid FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & toDeleteID & "') OR (userid = '" & toDeleteID & "' AND friendid = '" & UserID & "') LIMIT 1") = True Then '// If there's a friendship between those users
            If HoloMANAGERS.isOnline(toDeleteID) Then HoloMANAGERS.getUserClass(toDeleteID).transData("BJI" & HoloENCODING.encodeVL64(UserID) & sysChar(1)) '// If the deleted friend is online, then send a packet that removes the user from his list

            HoloDB.runQuery("DELETE FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & toDeleteID & "') OR (userid = '" & toDeleteID & "' AND friendid = '" & UserID & "') LIMIT 1") '// Update the messenger_friendships table
            HoloDB.runQuery("DELETE FROM messenger_messages WHERE (userid = '" & UserID & "' AND friendid = '" & toDeleteID & "') OR (userid = '" & toDeleteID & "' AND friendid = '" & UserID & "') LIMIT 1") '// Update the messenger_messages table; delete all pending messages between those users
        End If
    End Sub
    Private Sub Console_SendMessage()
        Dim cntReceivers As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2)) '// Get the count of receivers for this message
        Dim receiverIDs(200) As Integer '// Create a static array with max member count 200 so users can't send a message to more than 200 users at once

        currentPacket = currentPacket.Substring(2 + HoloENCODING.encodeVL64(cntReceivers).Length) '// Get the length of the encoded count of receivers, and cut it off the packet together with the header, so we don't include that part in our encoding/decoding cycle all the time
        For x = 0 To cntReceivers - 1 '// Start a loop to get all IDs the user wants to send the message to
            receiverIDs(x) = HoloENCODING.decodeVL64(currentPacket) '// Get the next ID in row by decoding the remaining packet (yes, decoding the whole row will give us the value of the first encoded ID in row ;])
            currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(receiverIDs(x)).Length) '// Encode the found ID which you just decoded, to and check the length of it's encoded variant, and cut that piece off the remaining packet
        Next '// Attempt to get the next one

        Dim messageText As String = currentPacket.Substring(2) '// Get the message itself which is left over in the packet after retrieving the IDs
        Dim messageTimeStamp As String = DateTime.Now.ToString '// Get the current date+time so all messages are 'sent at same time'
        Dim messagePlaceHolderStamp As String = "holo.pmaster.newmail_ph=" & mainHoloWINDOWS.rndVal(1, 1250) & "-" & mainHoloWINDOWS.rndVal(150, 1150) '// Create a fully random placeholder for this message, so we don't insert the full message in the database all the time but we just set a placeholder and use UPDATE later =]

        '// Insert the messages in the database
        Dim receiversMessageCount As Integer
        For m = 0 To cntReceivers - 1 '// Attempt to send the message to all receivers the user wanted
            If HoloDB.checkExists("SELECT userid FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & receiverIDs(m) & "') OR (userid = '" & receiverIDs(m) & "' AND friendid = '" & UserID & "') LIMIT 1") = True Then '// If there is a friendship between those two users (so random packet senders: stop here)
                receiversMessageCount = Val(HoloDB.runRead("SELECT MAX(messageid) FROM messenger_messages WHERE userid = '" & receiverIDs(m) & "'")) + 1 '// Get the users new message count
                If HoloMANAGERS.isOnline(receiverIDs(m)) = True Then HoloMANAGERS.getUserClass(receiverIDs(m)).transData("BF" & HoloENCODING.encodeVL64(receiversMessageCount) & HoloENCODING.encodeVL64(UserID) & messageTimeStamp & sysChar(2) & messageText & sysChar(2) & sysChar(1)) '// If the receiver is online, then send him the new message
                HoloDB.runQuery("INSERT INTO messenger_messages(userid,friendid,messageid,sent_on,message) VALUES ('" & receiverIDs(m) & "','" & UserID & "','" & receiversMessageCount & "','" & messageTimeStamp & "','" & messagePlaceHolderStamp & "')") '// Insert this a place holder message + additional message data in the database
            End If
        Next

        HoloDB.runQuery("UPDATE messenger_messages SET message = '" & HoloDB.fixChars(messageText) & "' WHERE message = '" & messagePlaceHolderStamp & "' LIMIT " & cntReceivers) '// Update the database, replacing all the current placeholder fields with a char-fixed (') copy of the message, this way we don't have to perform xx big message queries all the time
    End Sub
    Private Sub Console_DeleteMessage()
        Dim messageToDeleteID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2)) '// Get the ID of the message the user wants to delete
        HoloDB.runQuery("DELETE FROM messenger_messages WHERE userid = '" & UserID & "' AND messageid = '" & messageToDeleteID & "' LIMIT 1") '// Delete the message from the database
    End Sub
    Private Sub Console_FriendFollow()
        Dim friendToStalkID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
        If HoloDB.checkExists("SELECT userid FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & friendToStalkID & "') OR (userid = '" & friendToStalkID & "' AND friendid = '" & UserID & "') LIMIT 1") = False Then transData("E]H" & sysChar(1)) : Return
        If HoloMANAGERS.isOnline(friendToStalkID) = True Then
            Dim friendToStalkDetails As clsHoloUSERDETAILS = HoloMANAGERS.getUserDetails(friendToStalkID)
            If friendToStalkDetails.roomID > 0 Then
                transData("D^H" & HoloENCODING.encodeVL64(friendToStalkDetails.roomID) & sysChar(1))
            Else
                transData("E]J" & sysChar(1))
            End If
        Else
            transData("E]I" & sysChar(1))
        End If
    End Sub
    Private Sub Console_UpdateStats()
        Dim currentFriendID, friendCount As Integer
        Dim updatedFriendsPack As New StringBuilder
        Dim friendIDs() As String = HoloDB.runReadArray("SELECT userid,friendid FROM messenger_friendships WHERE (userid = '" & UserID & "') OR (friendid = '" & UserID & "')", True)

        For f = 0 To friendIDs.Count - 1
            If friendIDs(f).Split(sysChar(9))(0) = UserID.ToString Then currentFriendID = Integer.Parse(friendIDs(f).Split(sysChar(9))(1)) Else currentFriendID = Integer.Parse(friendIDs(f).Split(sysChar(9))(0))
            If currentFriendID > 0 Then
                updatedFriendsPack.Append(HoloENCODING.encodeVL64(currentFriendID))
                If HoloMANAGERS.isOnline(currentFriendID) = True Then '// If the user is online
                    updatedFriendsPack.Append(HoloMANAGERS.getUserClass(currentFriendID).userDetails.consoleMission & sysChar(2) & HoloMANAGERS.getUserHotelPosition(currentFriendID) & sysChar(2))
                Else
                    Dim friendsDetails() As String = HoloDB.runReadArray("SELECT consolemission,lastvisit FROM users WHERE id = '" & currentFriendID & "' LIMIT 1")
                    updatedFriendsPack.Append(friendsDetails(0) & sysChar(2) & "H" & friendsDetails(1) & sysChar(2))
                End If
                friendCount += 1
            End If
        Next

        transData("@M" & HoloENCODING.encodeVL64(friendCount) & updatedFriendsPack.ToString & sysChar(1))
    End Sub
#End Region
#Region "Hotel Navigator actions"
    Private Sub handleNavigatorAction()
        Dim catID As String = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get the ID of the part the user wants to see
        Dim hideFullRooms As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1))
        Dim curCatData() As String = HoloDB.runReadArray("SELECT parent,name,ispubcat FROM nav_categories WHERE id = '" & catID & "' LIMIT 1")
        Dim navigatorPack As New StringBuilder
        If curCatData.Count = 0 Then transData("C\" & HoloENCODING.encodeVL64(hideFullRooms) & HoloENCODING.encodeVL64(catID) & "H" & "Error!" & sysChar(2) & sysChar(1)) : Return

        navigatorPack.Append("C\" & HoloENCODING.encodeVL64(hideFullRooms) & HoloENCODING.encodeVL64(catID) & "H" & curCatData(1) & sysChar(2))

        If curCatData(2) = "1" Then '// Public Room category
            navigatorPack.Append("HPrK")

            '// Get all publicrooms in this category
            Dim publicRooms() As String = HoloDB.runReadArray("SELECT id,name_cct,name_caption,name_icon,incnt_now,incnt_max FROM publicrooms WHERE category_in = '" & catID & "' ORDER BY id ASC", True)
            For p = 0 To publicRooms.Count - 1
                Dim roomData() As String = publicRooms(p).Split(sysChar(9))
                If (hideFullRooms = 0) Or (hideFullRooms = 1 And Integer.Parse(roomData(4)) < Integer.Parse(roomData(5))) Then
                    navigatorPack.Append(HoloENCODING.encodeVL64(roomData(0)) & "I" & roomData(2) & sysChar(2) & HoloENCODING.encodeVL64(roomData(4)) & HoloENCODING.encodeVL64(roomData(5)) & "K" & roomData(3) & sysChar(2) & HoloENCODING.encodeVL64(roomData(0)) & "H" & roomData(1) & sysChar(2) & "HI")
                End If
            Next

            '// Get all categories in this category
            Dim publicCategories() As String = HoloDB.runReadArray("SELECT id,name FROM nav_categories WHERE parent = '" & catID & "'", True)
            For p = 0 To publicCategories.Count - 1
                Dim categoryData() As String = publicCategories(p).Split(sysChar(9))
                navigatorPack.Append(HoloENCODING.encodeVL64(categoryData(0)) & "J" & categoryData(1) & sysChar(2) & HoloENCODING.encodeVL64(0) & HoloENCODING.encodeVL64(1000) & HoloENCODING.encodeVL64(catID) & "H")
            Next
        Else '// Guestroom category
            navigatorPack.Append("XFAcJCAI")

            '// Get all the categories in this category
            Dim guestCategories() As String = HoloDB.runReadArray("SELECT id,name FROM nav_categories WHERE parent = '" & catID & "'", True)
            For c = 0 To guestCategories.Count - 1
                Dim categoryData() As String = guestCategories(c).Split(sysChar(9))
                navigatorPack.Append(HoloENCODING.encodeVL64(categoryData(0)) & "J" & categoryData(1) & sysChar(2) & HoloENCODING.encodeVL64(0) & HoloENCODING.encodeVL64(500) & "PAH")
            Next

            '// Get all guestrooms in this category
            Dim guestRooms() As String = HoloDB.runReadArray("SELECT id,name,owner,descr,state,showname,incnt_now,incnt_max FROM guestrooms WHERE category_in = '" & catID & "' ORDER BY incnt_now DESC LIMIT 30", True)
            Dim guestRoomPack As New StringBuilder
            For g = 0 To guestRooms.Count - 1
                Dim roomData() As String = guestRooms(g).Split(sysChar(9))
                If roomData(5) = "0" Then If Not (roomData(2) = userDetails.Name) And HoloRANK(userDetails.Rank).containsRight("fuse_enter_locked_rooms") = False Then roomData(2) = "-"
                guestRoomPack.Append(HoloENCODING.encodeVL64(roomData(0)) & roomData(1) & sysChar(2) & roomData(2) & sysChar(2) & HoloMISC.getRoomState(roomData(4)) & sysChar(2) & HoloENCODING.encodeVL64(roomData(6)) & HoloENCODING.encodeVL64(roomData(6)) & roomData(3) & sysChar(2))
            Next
            If guestRooms.Count > 0 Then navigatorPack.Append(HoloENCODING.encodeVL64(guestRooms.Count * 15) & HoloENCODING.encodeVL64(catID) & HoloENCODING.encodeVL64(guestRooms.Count) & guestRoomPack.ToString & sysChar(2))
        End If

        navigatorPack.Append(sysChar(1))
        transData(navigatorPack.ToString)
    End Sub
    Private Sub handleNavigatorAction_RecRooms()
        Dim getCycles As Integer
        Dim maxRoomID As Integer = HoloDB.runRead("SELECT MAX(id) FROM guestrooms")
        Dim roomPack As New StringBuilder
        Dim v As New Random

        Try
            For r = 1 To 3
reGrabID:
                If getCycles = 10 Then Return '// Too many errors/no rooms found
                getCycles += 1

                Dim roomID As Integer = v.Next(101, maxRoomID + 1)
                Dim roomData() As String = HoloDB.runReadArray("SELECT name,owner,descr,state,incnt_now,incnt_max FROM guestrooms WHERE id = '" & roomID & "' AND showname = '1' LIMIT 1")
                If roomData.Count = 0 Then GoTo reGrabID '// Non-valid room, regrab!

                roomPack.Append(HoloENCODING.encodeVL64(roomID) & roomData(0) & sysChar(2) & roomData(1) & sysChar(2) & HoloMISC.getRoomState(roomData(3)) & sysChar(2) & HoloENCODING.encodeVL64(roomData(4)) & HoloENCODING.encodeVL64(roomData(5)) & roomData(2) & sysChar(2))
            Next
            Console.WriteLine("E_" & HoloENCODING.encodeVL64(3) & roomPack.ToString & sysChar(2) & sysChar(1))
            transData("E_" & HoloENCODING.encodeVL64(3) & roomPack.ToString & sysChar(2) & sysChar(1))

        Catch
            transData("E_H" & sysChar(1))

        End Try
    End Sub
    Private Sub searchRoom()
        Dim searchQuery As String = currentPacket.Substring(2)
        Dim matchingRooms() As String = HoloDB.runReadArray("SELECT id,name,owner,descr,state,showname,incnt_now,incnt_max FROM guestrooms WHERE owner = '" & searchQuery & "' OR name LIKE '%" & searchQuery & "%' ORDER BY id ASC LIMIT 15", True)
        If matchingRooms.Count > 0 Then
            Dim searchResult As New StringBuilder("@w")
            For r = 0 To matchingRooms.Count - 1
                Dim roomData() As String = matchingRooms(r).Split(sysChar(9))
                If roomData(5) = "0" Then If Not (roomData(2) = userDetails.Name) And HoloRANK(userDetails.Rank).containsRight("fuse_enter_locked_rooms") = False Then roomData(2) = "-"
                searchResult.Append(roomData(0) & sysChar(9) & roomData(1) & sysChar(9) & roomData(2) & sysChar(9) & HoloMISC.getRoomState(roomData(4)) & sysChar(9) & "x" & sysChar(9) & roomData(6) & sysChar(9) & roomData(7) & sysChar(9) & "null" & sysChar(9) & roomData(3) & sysChar(9) & sysChar(13))
            Next
            transData(searchResult.ToString & sysChar(1))
        Else
            transData("@z" & sysChar(1))
        End If
    End Sub
    Private Sub seeMyRooms()
        Dim myRooms() As String = HoloDB.runReadArray("SELECT id,name,descr,state,showname,incnt_now,incnt_max FROM guestrooms WHERE owner = '" & userDetails.Name & "' ORDER BY id ASC", True)
        If myRooms.Count > 0 Then
            Dim myRoomPack As New StringBuilder("@P")
            For r = 0 To myRooms.Count - 1
                Dim roomData() As String = myRooms(r).Split(sysChar(9))
                myRoomPack.Append(roomData(0) & sysChar(9) & roomData(1) & sysChar(9) & userDetails.Name & sysChar(9) & HoloMISC.getRoomState(roomData(3)) & sysChar(9) & "x" & sysChar(9) & roomData(5) & sysChar(9) & roomData(6) & sysChar(9) & "null" & sysChar(9) & roomData(2) & sysChar(9) & sysChar(13))
            Next

            transData(myRoomPack.ToString & sysChar(1))
        Else
            transData("@y" & userDetails.Name & sysChar(1))
        End If
    End Sub
    Private Sub checkVoucher()
        Dim voucherCode As String = currentPacket.Substring(4)
        Dim voucherAmount As Integer = Val(HoloDB.runRead("SELECT credits FROM vouchers WHERE voucher = '" & voucherCode & "' LIMIT 1"))
        If voucherAmount > 0 Then
            Dim newCredits As Integer = Integer.Parse(HoloDB.runRead("SELECT credits FROM users WHERE id = '" & UserID & "' LIMIT 1")) + voucherAmount
            transData("CT" & sysChar(1) & "@F" & newCredits & ".0" & sysChar(1))
            HoloDB.runQuery("DELETE FROM vouchers WHERE voucher = '" & voucherCode & "' LIMIT 1")
            HoloDB.runQuery("UPDATE users SET credits = '" & newCredits & "' WHERE id = '" & UserID & "' LIMIT 1")
        Else
            transData("CU1" & sysChar(1))
        End If
    End Sub
    Private Sub roomModifier(ByRef actionID As Byte)
        Select Case actionID

            Case 1 '// Create a room - phase 1
                Dim roomSettings() As String = currentPacket.Split("/")
                If Integer.Parse(HoloDB.runRead("SELECT COUNT(id) FROM guestrooms WHERE owner = '" & userDetails.Name & "' LIMIT 1")) < 10 Then '// User already has less than 10 rooms
                    roomSettings(2) = HoloMISC.filterWord(roomSettings(2), userDetails.Rank) '// Pass the roomname through the wordfilter
                    roomSettings(3) = HoloMISC.getRoomModelID(Char.Parse(roomSettings(3).Substring(6, 1)))
                    roomSettings(4) = HoloMISC.getRoomState(roomSettings(4), True) '// Get the ID of the state of the room for use with database

                    HoloDB.runQuery("INSERT INTO guestrooms (name,owner,model,state,showname) VALUES ('" & HoloDB.fixChars(roomSettings(2)) & "','" & userDetails.Name & "','" & roomSettings(3) & "','" & roomSettings(4) & "','" & roomSettings(5) & "')")
                    Dim roomID As String = HoloDB.runRead("SELECT MAX(id) FROM guestrooms WHERE owner = '" & userDetails.Name & "' LIMIT 1")
                    transData("@{" & roomID & sysChar(13) & roomSettings(2) & sysChar(1))
                Else
                    transData("@a" & "Error creating a private room" & sysChar(1)) '// Alert!
                End If

            Case 2 '// Create guestroom - phase 2 / Modify guestroom
                Dim roomID As Integer
                Dim packetContent() As String = currentPacket.Split(sysChar(13))
                If currentPacket.Substring(2, 1) = "/" Then '// Create guestroom - phase 2
                    roomID = currentPacket.Split("/")(1) '// Room ID
                    packetContent(1) = HoloDB.fixChars(HoloMISC.filterWord(packetContent(1).Substring(12), userDetails.Rank)) '// Room description
                    packetContent(2) = HoloDB.fixChars(packetContent(2).Substring(9)).Trim '// Room password
                    packetContent(3) = Integer.Parse(packetContent(3).Substring(13)) '// Everyone rights here? (superusers 1/0)
                    HoloDB.runQuery("UPDATE guestrooms SET descr = '" & packetContent(1) & "',opt_password = '" & packetContent(2) & "',superusers = '" & packetContent(3) & "' WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "' LIMIT 1") '// Run an update query at the database, if the user is the real owner of this room then the stuff will get sorted out nicely, if not, nothing happens (it just doesn't finds a matching row)
                Else '// Modify guestroom (save)
                    roomID = currentPacket.Substring(2).Split("/")(0) '// Room ID
                    packetContent(1) = HoloDB.fixChars(HoloMISC.filterWord(packetContent(1).Substring(12), userDetails.Rank)) '// Room description
                    packetContent(2) = Integer.Parse(packetContent(2).Substring(13)) '// Everyone rights here? (superusers 1/0) Try to parse to integer to check validity
                    packetContent(3) = Integer.Parse(packetContent(3).Substring(12)) '// Max users inside room at same time. Try to parse to integer to check validity
                    HoloDB.runQuery("UPDATE guestrooms SET descr = '" & packetContent(1) & "',superusers = '" & packetContent(2) & "',incnt_max = '" & packetContent(3) & "' WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "' LIMIT 1") '// Run an update query at the database, if the user is the real owner of this room then the stuff will get sorted out nicely, if not, nothing happens (it just doesn't finds a matching row)
                End If

            Case 3 '// Modify guestroom, get category (BX packet, request)
                Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim roomCategory As String = HoloDB.runRead("SELECT category_in FROM guestrooms WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "' LIMIT 1")
                If roomCategory.Count > 0 Then transData("C^" & HoloENCODING.encodeVL64(roomID) & HoloENCODING.encodeVL64(roomCategory) & sysChar(1))

            Case 4 '// Modify guestroom - save name, state and show/hide ownername (@X)
                Dim packetContent() As String = currentPacket.Substring(2).Split("/")
                packetContent(1) = HoloDB.fixChars(HoloMISC.filterWord(packetContent(1), userDetails.Rank))
                packetContent(2) = HoloMISC.getRoomState(packetContent(2), True)
                HoloDB.runQuery("UPDATE guestrooms SET name = '" & packetContent(1) & "',state = '" & packetContent(2) & "',showname = '" & Integer.Parse(packetContent(3)) & "' WHERE id = '" & packetContent(0) & "' AND owner = '" & userDetails.Name & "' LIMIT 1")

            Case 5 '// Delete a room
                Dim roomID As Integer = currentPacket.Substring(2)
                If HoloDB.checkExists("SELECT id FROM guestrooms WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "' LIMIT 1") = True Then '// If there exists a room with this ID and the owner is this user [so this user owns this room]
                    HoloDB.runQuery("DELETE FROM guestrooms WHERE id = '" & roomID & "' LIMIT 1") '// Delete this room from database
                    HoloDB.runQuery("DELETE FROM furniture WHERE roomid = '" & roomID & "'") '// Delete all the furniture in this room from database
                    HoloDB.runQuery("DELETE FROM furniture_moodlight WHERE roomid = '" & roomID & "' LIMIT 1") '// Delete the moodlight presets of the moodlight in this room [if any]
                End If

            Case 6 '// Reset roomrights
                Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                If HoloDB.checkExists("SELECT id FROM guestrooms WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "' LIMIT 1") = True Then
                    HoloDB.runQuery("DELETE FROM guestroom_rights")
                    '// Blabla update their status in the room etc, mehmeh
                End If

        End Select
    End Sub
    Private Sub Favourites_Init()
        Dim myFavs() As String = HoloDB.runReadArray("SELECT roomid FROM nav_favrooms WHERE userid = '" & UserID & "' ORDER BY ispublicroom LIMIT 30", True)
        Dim myFavTypes() As String = HoloDB.runReadArray("SELECT ispublicroom FROM nav_favrooms WHERE userid = '" & UserID & "' ORDER by ispublicroom LIMIT 30", True)
        Dim deletedFavCount As Integer
        Dim roomPack As New StringBuilder

        For f = 0 To myFavs.Count - 1
            If myFavTypes(f) = 1 Then '// This fav is a publicroom
                Dim roomData() As String = HoloDB.runReadArray("SELECT name_cct,name_caption,name_icon,incnt_now,incnt_max FROM publicrooms WHERE id = '" & myFavs(f) & "' LIMIT 1")
                If roomData.Count = 0 Then '// Non-existing room
                    deletedFavCount += 1
                    HoloDB.runQuery("DELETE FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & myFavs(f) & "' AND ispublicroom = '1' LIMIT 1") '// Remove this favourite room from users list
                Else '// Room exists, add it's details
                    roomPack.Append(HoloENCODING.encodeVL64(myFavs(f)) & "I" & roomData(1) & sysChar(2) & HoloENCODING.encodeVL64(roomData(3)) & HoloENCODING.encodeVL64(roomData(4)) & "I" & roomData(2) & sysChar(2) & HoloENCODING.encodeVL64(myFavs(f)) & "H" & roomData(0) & sysChar(2) & "IH")
                End If
            Else '// This fav is a guestroom
                Dim roomData() As String = HoloDB.runReadArray("SELECT name,owner,descr,state,showname,incnt_now,incnt_max FROM guestrooms WHERE id = '" & myFavs(f) & "' LIMIT 1")
                If roomData.Count = 0 Then '// Non-existing room
                    deletedFavCount += 1
                    HoloDB.runQuery("DELETE FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & myFavs(f) & "' AND ispublicroom = '0' LIMIT 1") '// Remove this favourite room from users list
                Else '// Room exists, add it's details
                    roomPack.Append(HoloENCODING.encodeVL64(myFavs(f)) & roomData(0) & sysChar(2) & roomData(1) & sysChar(2) & HoloMISC.getRoomState(roomData(3)) & sysChar(2) & HoloENCODING.encodeVL64(roomData(5)) & HoloENCODING.encodeVL64(roomData(6)) & roomData(2) & sysChar(2))
                End If
            End If
        Next

        transData("@}HHJ" & sysChar(2) & "HHH" & HoloENCODING.encodeVL64(myFavs.Count - deletedFavCount) & roomPack.ToString & sysChar(1))
    End Sub
    Private Sub Favourites_AddRoom()
        Dim isPub As Byte = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1)) '// Check if the user adds a guestroom or a publicroom
        Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get the ID of the room the user wants to add

        If HoloDB.checkExists("SELECT userid FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & roomID & "' AND ispublicroom = '" & isPub & "' LIMIT 1") = True Then Return '// Already added

        If isPub = 1 Then '// User adds publicroom
            If HoloDB.checkExists("SELECT id FROM publicrooms WHERE id = '" & roomID & "' LIMIT 1") = False Then Return '// This publicroom was not found, stop here
        Else '// User adds a guestroom
            If HoloDB.checkExists("SELECT id FROM guestrooms WHERE id = '" & roomID & "' LIMIT 1") = False Then Return '// This guestroom was not found, stop here
        End If

        If Integer.Parse(HoloDB.runRead("SELECT COUNT(*) FROM nav_favrooms WHERE userid = '" & UserID & "' LIMIT 1")) >= 30 Then '// The user already has 30 favourite rooms! (or even more for some reason)
            transData("@a" & "nav_error_toomanyfavrooms" & sysChar(1)) '// Send the message that the users list is full (external_texts)
            Return '// Stop here
        End If

        HoloDB.runQuery("INSERT INTO nav_favrooms(userid,roomid,ispublicroom) VALUES ('" & UserID & "','" & roomID & "','" & isPub & "')") '// Insert this favourite in the database
    End Sub
    Private Sub Favourites_DeleteRoom()
        Dim isPub As Byte = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1)) '// Check if the user deletes a guestroom or a publicroom
        Dim toDeleteID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get the ID of the room the user wants to delete
        HoloDB.runQuery("DELETE FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & toDeleteID & "' AND ispublicroom = '" & isPub & "' LIMIT 1") '// Delete this favourite from the database
    End Sub
#End Region
#Region "Guestroom actions"
    Private Sub GuestRoom_CheckID()
        Dim roomID As Integer = currentPacket.Substring(2)
        Dim roomData() As String = HoloDB.runReadArray("SELECT name,owner,descr,model,state,superusers,showname,incnt_now,incnt_max FROM guestrooms WHERE id = '" & roomID & "' LIMIT 1")
        If roomData.Count = 0 Then '// Non-existing room
            '// send error
        Else
            Dim allowTrading As Integer = 1 '// Meh we just load this from db later, this will make the 'Trade' button in rooms show up/hide
            transData("@v" & HoloENCODING.encodeVL64(roomData(5)) & HoloENCODING.encodeVL64(roomData(4)) & HoloENCODING.encodeVL64(roomID) & roomData(1) & sysChar(2) & "model_" & HoloMISC.getRoomModelChar(Byte.Parse(roomData(3))) & sysChar(2) & roomData(0) & sysChar(2) & roomData(2) & sysChar(2) & HoloENCODING.encodeVL64(roomData(6)) & HoloENCODING.encodeVL64(allowTrading) & "H" & HoloENCODING.encodeVL64(roomData(7)) & HoloENCODING.encodeVL64(roomData(8)) & sysChar(1))
        End If
    End Sub
    Private Sub GuestRoom_CheckState()
        Dim packetContent() As String = currentPacket.Substring(2).Split("/")
        Dim roomID As Integer = packetContent(0)
        Dim roomData() As String = HoloDB.runReadArray("SELECT owner,state,incnt_now,incnt_max FROM guestrooms WHERE id = '" & roomID & "' LIMIT 1")

        If roomData.Count = 0 Then Return '// Room does not exist (there was no row for it found)

        If Not (userDetails.Name = roomData(0)) And (HoloRANK(userDetails.Rank).containsRight("fuse_enter_all_rooms") = False) Then '// Someone who isn't owner/staff enters room
            If Integer.Parse(roomData(1)) > 0 Then '// This room is password/doorbell-ed
                If roomData(1) = "1" Then '// Doorbell room
                    If HoloMANAGERS.hookedRooms.ContainsKey(roomID) = True Then
                        Dim theRoom As clsHoloROOM = HoloMANAGERS.hookedRooms(roomID) '// Get class of this room
                        transData("A[" & sysChar(1)) '// Send the 'the doorbell is rang' message
                        theRoom.sendToRightHavingUsers("A[" & userDetails.Name & sysChar(1)) '// Send the 'USERNAME' has rang the doorbell message to all righthaving users in the room
                        Return '// Wait till you're being 'invited'
                    Else '// Room was not loaded; so there are no users inside!
                        Room_noRoom(False, True) '// Kick the user who wants to enter, who is gonna wait for a doorbell room without users inside? xD
                    End If
                Else '// Password room
                    If Not (HoloDB.runRead("SELECT opt_password FROM guestrooms WHERE roomid = '" & UserID & "' LIMIT 1") = packetContent(1)) Then transData("@a" & "Incorrect flat password" & sysChar(1)) : Return '// Wrong password entered, notify user and stop here
                End If

                '// Check if the user who enters has rights
                If HoloDB.checkExists("SELECT userid FROM guestroom_rights WHERE userid = '" & UserID & "' AND roomid = '" & roomID & "' LIMIT 1") = True Then
                    userDetails.addStatus("flatctrl", vbNullString)
                    userDetails.hasRights = True
                End If
            End If
            If Integer.Parse(roomData(2)) >= Integer.Parse(roomData(3)) Then transData("C`I" & sysChar(1) & "@R" & sysChar(1)) : Return '// Room is full, notify user and stop here

        Else '// Staff or owner enters room
            If userDetails.Name = roomData(0) Or HoloRANK(userDetails.Rank).containsRight("fuse_any_room_controller") = True Then
                userDetails.addStatus("flatctrl", "useradmin")
                userDetails.hasRights = True
                userDetails.isOwner = True
            End If
        End If

        userDetails.isAllowedInRoom = True
        If userDetails.isOwner = True Then transData("@o" & sysChar(1))
        If userDetails.hasRights = True Then transData("@j" & sysChar(1))

        transData("@i" & sysChar(1)) '// Send the 'proceed' packet
    End Sub
    Private Sub Room_rotateMe()
        Dim ToX As Integer = currentPacket.Substring(2).Split(" ")(0)
        Dim ToY As Integer = currentPacket.Split(" ")(1)
        With userDetails
            If .PosY > ToY Then .rotHead = 0
            If .PosX < ToX Then .rotHead = 2
            If .PosY < ToY Then .rotHead = 4
            If .PosX > ToX Then .rotHead = 6
            If .PosX < ToX And .PosY > ToY Then .rotHead = 1
            If .PosX < ToX And .PosY < ToY Then .rotHead = 3
            If .PosX > ToX And .PosY < ToY Then .rotHead = 5
            If .PosX > ToX And .PosY > ToY Then .rotHead = 7
            .rotBody = .rotHead
        End With
        roomCommunicator.refreshUser(userDetails)
    End Sub
    Private Sub Room_Talk()
        Dim talkType As Char
        Dim talkMessage As String = currentPacket.Substring(4)

        Select Case currentPacket.Substring(1, 1)
            Case "t"
                talkType = "X"

            Case "x"
                talkType = "Y"

            Case "w"
                talkType = "Z"
        End Select

        If talkMessage.Substring(0, 1) = ":" Then
            If handleSpeechCommand(talkMessage) = True Then Return
        End If

        '// Deal with the room about the message
        If HoloRACK.wordFilter_Enabled = True Then talkMessage = HoloMISC.filterWord(talkMessage, userDetails.Rank)
        roomCommunicator.doChat(userDetails, talkType, talkMessage)

        If HoloRACK.Chat_Animations = True Then '// Chat animations wanted?
            Dim persnlGesture As String
            talkMessage = talkMessage.ToLower

            If talkMessage.Contains(":)") Or talkMessage.Contains(":-)") Or talkMessage.Contains("=]") Or talkMessage.Contains(";)") Or talkMessage.Contains(";-)") Or talkMessage.Contains(":d") Or talkMessage.Contains(":p") Then
                persnlGesture = "sml"
            ElseIf talkMessage.Contains(":(") Or talkMessage.Contains(":-(") Or talkMessage.Contains(":s") Then
                persnlGesture = "sad"
            ElseIf talkMessage.Contains(":o") Or talkMessage.Contains(":-o") Then
                persnlGesture = "srp"
            ElseIf talkMessage.Contains(":@") Or talkMessage.Contains(":-@") Then
                persnlGesture = "agr"
            End If

            userDetails.showTalkAnimation((talkMessage.Length + 50) * 30, persnlGesture)
        End If
    End Sub
    Friend Sub Room_noRoom(ByVal removeFromRoomClass As Boolean, ByVal sendKick As Boolean)
        If removeFromRoomClass = True Then If IsNothing(roomCommunicator) = False Then roomCommunicator.leaveUser(userDetails)
        If IsNothing(userDetails) = False Then userDetails.Reset()
        If sendKick = True Then transData("@R" & sysChar(1)) '// Send the kick packet
    End Sub
    Private Sub refreshHand(ByVal strMode As String)
        Dim startID, stopID As Integer
        Dim itemIDs() As String = HoloDB.runReadArray("SELECT id FROM furniture WHERE inhand = '" & UserID & "'", True)
        Dim handPack As New StringBuilder("BL")

        stopID = itemIDs.Count
        Select Case strMode '// If strMode is 'update', then it doesn't do anything here and the current pagenumber will stay the same, which is what we want :D
            Case "next"
                curHandPage += 1
            Case "prev"
                curHandPage -= 1
            Case "last"
                curHandPage = (stopID - 1) / 9
            Case "new"
                curHandPage = 0
        End Select

        If itemIDs.Count > 0 Then
reCount:
            startID = curHandPage * 9
            If stopID > (startID + 9) Then stopID = startID + 9
            If (startID > stopID) Or (startID = stopID) Then curHandPage -= 1 : GoTo reCount

            For i = startID To stopID - 1
                Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemIDs(i) & "' LIMIT 1")
                Dim recycleFlag As Byte = 1 '// If the 'is-recycle-able' icon should blink up (will do Recycler later, it's easy + I've made it before) } Nillus
                handPack.Append("SI" & sysChar(30) & itemIDs(i) & sysChar(30) & i & sysChar(30))
                If HoloITEM(templateID).typeID = 0 Then handPack.Append("I") Else handPack.Append("S")
                handPack.Append(sysChar(30) & itemIDs(i) & sysChar(30) & HoloITEM(templateID).cctName & sysChar(30))
                If HoloITEM(templateID).typeID > 0 Then handPack.Append(HoloITEM(templateID).Length & sysChar(30) & HoloITEM(templateID).Width & sysChar(30) & HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemIDs(i) & "' LIMIT 1") & sysChar(30))
                handPack.Append(HoloITEM(templateID).Colour & sysChar(30) & recycleFlag & sysChar(30) & "/")
                'SEEMS NOT NEEDED, HOWEVER REAL HABBO SENDS IT :(...} If HoloITEM(templateID).typeID > 0 Then handPack.Append(HoloITEM(templateID).cctName & sysChar(30))
                'handPack.Append("/")
            Next
        End If
        handPack.Append(sysChar(13) & itemIDs.Count & sysChar(1))
        transData(handPack.ToString)

        'MsgBox(handPack.ToString.Replace(sysChar(30), "{}"))
        '// DEBUG! :D
        '//    '// SI+ {30} + -ID + {30} + iI + {30} + TYPE + {30} + ID + {30} + CCT + {30} + Len + {30} + Wid + {30} + VAR + {30} + COLOR + {30} + iI + {30} + CCT + {30} + "/"
        '// Console.WriteLine(startID & " - " & stopID & " |HANDPAGE: " & curHandPage)
    End Sub
    Private Function handleSpeechCommand(ByVal talkMessage As String) As Boolean
        Dim commandPart() = talkMessage.Split(" ")
        Dim theCommand As String = commandPart(0).Substring(1)
        If commandPart.Count = 1 Then
            Select Case theCommand

                Case ":about"
                    MsgBox(userDetails.roomUID)
                    transData("BK" & "Hey " & userDetails.Name & ", you currently are on a Holograph Emulator for Habbo Hotel!\r\rWe forgot something? What the hell?\rOh yes!\r'Hello world!\r\r- Nillus and co" & sysChar(1))
                    Return True

            End Select
        Else
            Select Case theCommand
                Case "hello"
                    transData("BK" & "Hello, you said " & commandPart(1) & "!" & sysChar(1))
                    Return True

                Case "reload"
                    If userDetails.Rank = 7 Then
                        Select Case commandPart(1)
                            Case "catalogue"
                                mainHoloAPP.cacheCatalogue()
                                Return True

                            Case "somecommand"
                                Return True
                        End Select
                    End If
            End Select
        End If

        Return False
    End Function
#End Region
#Region "Games"
    Private Sub resetGameStatuses()
        With userDetails
            .Game_ID = -1
            .Game_owns = False
            .Game_withState = -1
        End With
    End Sub
#End Region
#End Region
End Class
