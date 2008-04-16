Imports System.Text
Imports System.Collections
Public Class HoloCLIENT
    Friend Class catalogueManager
        Private Shared cataloguePages As Hashtable
        Private Shared itemCache As Hashtable
        Friend Shared Sub Init()
            Console.WriteLine(vbNullString)
            Console.WriteLine("[HOLOCACHE] Starting caching of catalogue + items, this may take a while...")

            Dim pageIDs() As Integer = HoloDB.runReadColumn("SELECT indexid FROM catalogue_pages ORDER BY indexid", 0, Nothing)
            cataloguePages = New Hashtable()
            itemCache = New Hashtable()

            For i = 0 To pageIDs.Count - 1
                cachePage(pageIDs(i))
            Next
            cachePage(-1) '// Cache all the items that aren't on a page, but require to be cached to prevent that already bought instances will appear as PH box. Items like this have catalogue_page_id = -1 in the catalogue_items table

            Console.WriteLine("[CATMGR] Successfully cached " & cataloguePages.Count & " catalogue pages and " & itemCache.Count & " item templates!")
            Console.WriteLine(vbNullString)
        End Sub
        Private Shared Sub cachePage(ByVal pageID As Integer)
            Dim pageData() As String = HoloDB.runReadRow("SELECT indexname,minrank,displayname,style_layout,img_header,img_side,label_description,label_misc,label_moredetails FROM catalogue_pages WHERE indexid = '" & pageID & "'")
            If pageID > 0 Then If pageData.Count = 0 Then Return

            Dim pageBuilder As New System.Text.StringBuilder
            Dim pageIndexName As String = vbNullString
            Dim objPage As New cataloguePage()

            If pageID > 0 Then '// If it's a page + items, and not just not-on-page items, then cache the page
                pageIndexName = pageData(0)
                objPage.displayName = pageData(2) '// Set display name for this page
                objPage.minRank = Byte.Parse(pageData(1))

                pageBuilder.Append("i:" & pageIndexName & Convert.ToChar(13) & "n:" & pageData(2) & Convert.ToChar(13) & "l:" & pageData(3) & Convert.ToChar(13)) '// Add the required fields for catalogue page (indexname, showname, page layout style (boxes etc))
                If Not (pageData(4)) = vbNullString Then pageBuilder.Append("g:" & pageData(4) & Convert.ToChar(13)) '// If there's a headline image set, add it
                If Not (pageData(5)) = vbNullString Then pageBuilder.Append("e:" & pageData(5) & Convert.ToChar(13)) '// If there is/are side image(s) set, add it/them
                If Not (pageData(6)) = vbNullString Then pageBuilder.Append("h:" & pageData(6) & Convert.ToChar(13)) '// If there's a description set, add it
                If Not (pageData(8)) = vbNullString Then pageBuilder.Append("w:" & pageData(8) & Convert.ToChar(13)) '// If there's a 'Click here for more details' label set, add it
                If Not (pageData(7)) = vbNullString Then '// If the misc additions field is not blank
                    Dim miscDetail() As String = pageData(7).Split(vbCrLf) '// Split the misc additions field to string array
                    For m = 0 To miscDetail.Count - 1 : pageBuilder.Append(miscDetail(m) & Convert.ToChar(13)) : Next '// Go along all misc additions and add them, followed by Char13
                End If
            End If

            Dim itemTemplateIDs() As Integer = HoloDB.runReadColumn("SELECT tid FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemTypeIDs() As Integer = HoloDB.runReadColumn("SELECT typeid FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemLengths() As Integer = HoloDB.runReadColumn("SELECT length FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemWidths() As Integer = HoloDB.runReadColumn("SELECT width FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemCosts() As Integer = HoloDB.runReadColumn("SELECT catalogue_cost FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemDoorFlags() As Integer = HoloDB.runReadColumn("SELECT door FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemTradeableFlags() As Integer = HoloDB.runReadColumn("SELECT tradeable FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemRecycleAbleFlags() As Integer = HoloDB.runReadColumn("SELECT recycleable FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0, Nothing)
            Dim itemNames() As String = HoloDB.runReadColumn("SELECT catalogue_name FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0)
            Dim itemDescs() As String = HoloDB.runReadColumn("SELECT catalogue_description FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0)
            Dim itemCCTs() As String = HoloDB.runReadColumn("SELECT name_cct FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0)
            Dim itemColours() As String = HoloDB.runReadColumn("SELECT colour FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0)
            Dim itemTopHs() As String = HoloDB.runReadColumn("SELECT top FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", 0)

            For i = 0 To itemTemplateIDs.Count - 1
                If substringIs(itemCCTs(i), "deal", 0, 4) = False Then '// Not a deal =]
                    Dim tmpItemTemplate As New cachedItem()
                    tmpItemTemplate.Init(itemCCTs(i), itemTypeIDs(i), itemColours(i), itemLengths(i), itemWidths(i), Double.Parse(itemTopHs(i)), (itemDoorFlags(i) = 1), (itemTradeableFlags(i) = 1), (itemRecycleAbleFlags(i) = 1))
                    itemCache.Add(itemTemplateIDs(i), tmpItemTemplate)

                    If pageID = -1 Then Continue For '// A 'not on page, just for caching'-item, no page is being made, so not adding it to page

                    pageBuilder.Append("p:" & itemNames(i) & Convert.ToChar(9) & itemDescs(i) & Convert.ToChar(9) & itemCosts(i) & Convert.ToChar(9) & Convert.ToChar(9)) '// Add the common fields for both wallitem/flooritem
                    If itemTypeIDs(i) = 0 Then pageBuilder.Append("i") Else pageBuilder.Append("s") '// Wallitem or flooritem? This will do the trick!!111
                    pageBuilder.Append(Convert.ToChar(9) & itemCCTs(i) & Convert.ToChar(9)) '// Add a char9 + the cctname + char9
                    If itemTypeIDs(i) = 0 Then pageBuilder.Append(Convert.ToChar(9)) Else pageBuilder.Append("0" & Convert.ToChar(9)) '// If wallitem, then just add a char9, if flooritem, then add a 0 + char9
                    If itemTypeIDs(i) = 0 Then pageBuilder.Append(Convert.ToChar(9)) Else pageBuilder.Append(itemLengths(i) & "," & itemWidths(i) & Convert.ToChar(9)) '// If wallitem, then just add a char9, if flooritem, then add the item's width, item's length and a char9
                    pageBuilder.Append(itemCCTs(i) & Convert.ToChar(9)) '// Add the cctname again + char9
                    If itemTypeIDs(i) > 0 Then pageBuilder.Append(itemColours(i)) '// If it's a flooritem, then add the colour
                    pageBuilder.Append(Convert.ToChar(13)) '// Add char13 to mark the end of the current item string
                Else
                    Dim dealID As Integer = itemCCTs(i).Substring(4)
                    Dim dealItemIDs() As Integer = HoloDB.runReadColumn("SELECT tid FROM catalogue_deals WHERE id = '" & dealID & "'", 0, Nothing)
                    Dim dealItemAmounts() As Integer = HoloDB.runReadColumn("SELECT amount FROM catalogue_deals WHERE id = '" & dealID & "'", 0, Nothing)

                    pageBuilder.Append("p:" & itemNames(i) & Convert.ToChar(9) & itemDescs(i) & Convert.ToChar(9) & itemCosts(i) & Convert.ToChar(9) & Convert.ToChar(9) & "d")
                    pageBuilder.Append(Convert.ToChar(9), 4)
                    pageBuilder.Append("deal" & dealID & Convert.ToChar(9) & Convert.ToChar(9) & dealItemIDs.Count & Convert.ToChar(9))

                    For y = 0 To dealItemIDs.Count - 1
                        Dim itemCCT As String = HoloDB.runRead("SELECT name_cct FROM catalogue_items WHERE tid = '" & dealItemIDs(y) & "'")
                        Dim itemColour As String = HoloDB.runRead("SELECT colour FROM catalogue_items WHERE tid = '" & dealItemIDs(y) & "'")
                        pageBuilder.Append(itemCCT & Convert.ToChar(9) & dealItemAmounts(y) & Convert.ToChar(9) & itemColour & Convert.ToChar(9))
                    Next
                End If
            Next

            If pageID = -1 Then Return '// No page being generated, just caching all items that aren't on pages, so stop here
            objPage.strPage = pageBuilder.ToString() '// Unfold the stringbuilder for the current page and stow it in the caching instance of this page his 'strPage' property
            cataloguePages.Add(pageIndexName, objPage) '// Add the current page cache instance to the hashtable
        End Sub
        Private Structure cataloguePage
            Friend displayName As String
            Friend strPage As String
            Friend minRank As Byte
        End Structure
        Friend Shared Function getPageExists(ByVal pageName As String) As Boolean
            Return cataloguePages.ContainsKey(cataloguePages.ContainsKey(pageName))
        End Function
        Friend Shared Function getPageIndex(ByVal userRank As Byte) As String
            Try
                Dim listBuilder As New StringBuilder()
                Dim pageNames() As String = HoloDB.runReadColumn("SELECT indexname FROM catalogue_pages WHERE minrank <= '" & userRank & "' ORDER BY indexid ASC", 0)

                For i = 0 To pageNames.Count - 1
                    If cataloguePages.ContainsKey(pageNames(i)) Then listBuilder.Append(pageNames(i) & Convert.ToChar(9) & DirectCast(cataloguePages(pageNames(i)), cataloguePage).displayName & Convert.ToChar(13))
                Next

                Return listBuilder.ToString()

            Catch
                Return Convert.ToChar(13)

            End Try
        End Function
        Friend Shared Function getPage(ByVal pageName As String, ByVal userRank As Byte) As String
            Try
                Dim objPage As cataloguePage = cataloguePages(pageName)
                If objPage.minRank > userRank Then Return vbNullString

                Return objPage.strPage

            Catch
                Return "holo.cast_catalogue.access_denied"

            End Try
        End Function
        Friend Shared Sub handleCatalogueSpecialItemAddition(ByVal templateID As Integer, ByVal receiverID As Integer, ByVal roomID As Integer, ByVal presentBoxID As Integer)
            Select Case HoloITEM(templateID).cctName
                Case "wallpaper", "floor" '// Wallpaper, floor
                    Dim decorID As Integer = roomID
                    Dim itemID As Integer = lastItemID()
                    HoloDB.runQuery("UPDATE furniture SET opt_var = '" & decorID & "' WHERE id = '" & itemID & "' LIMIT 1")
                    If presentBoxID > 0 Then HoloDB.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" & presentBoxID & "','" & itemID & "')") '// Add this item to the presentbox [if any]

                Case "roomdimmer" '// This item is a moodlight
                    Dim itemID As Integer = lastItemID()
                    Dim defaultPreset As String = "1,#000000,155" '// Edit to your wishes
                    Dim defaultSetPreset As String = "1,1,1,#000000,155" '// Edit to your wishes
                    HoloDB.runQuery("INSERT INTO furniture_moodlight(id,roomid,preset_cur,preset_1,preset_2,preset_3) VALUES ('" & itemID & "','0','1','" & defaultPreset & "','" & defaultPreset & "','" & defaultPreset & "')")
                    HoloDB.runQuery("UPDATE furniture SET opt_var = '" & defaultSetPreset & "' WHERE id = '" & itemID & "' LIMIT 1")
                    If presentBoxID > 0 Then HoloDB.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" & presentBoxID & "','" & itemID & "')") '// Add this item to the presentbox [if any]

                Case "door", "doorB", "doorC", "doorD", "teleport_door" '// This item is a teleporter, hand out another one [linking]
                    Dim itemID1 As Integer = lastItemID() '// Get the ID of teleporter 1
                    HoloDB.runQuery("INSERT INTO furniture(tid,ownerid,roomid,opt_teleportid) VALUES ('" & templateID & "','" & receiverID & "','" & roomID & "','" & itemID1 & "')") '// Create teleporter 2, linking to teleporter 1
                    Dim itemID2 As Integer = lastItemID()
                    HoloDB.runQuery("UPDATE furniture SET opt_teleportid = '" & itemID2 & "' WHERE id = '" & itemID1 & "' LIMIT 1") '// Set the linkid of teleporter 1 to the ID of teleporter 2
                    If presentBoxID > 0 Then
                        HoloDB.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" & presentBoxID & "','" & itemID1 & "')") '// Add teleporter 1 to the presentbox [if any]
                        HoloDB.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" & presentBoxID & "','" & itemID2 & "')") '// Add teleporter 2 to the presentbox [if any]
                    End If

                Case Else '// Not a special 'db action item', but if it's part of a present then add it to the box
                    If presentBoxID > 0 Then HoloDB.runQuery("INSERT INTO furniture_presents(id,itemid) VALUES ('" & presentBoxID & "','" & lastItemID() & "')")

            End Select
        End Sub
        Friend Shared ReadOnly Property lastItemID() As Integer
            Get
                Return HoloDB.runRead("SELECT MAX(id) FROM furniture", Nothing)
            End Get
        End Property
        Public Shared Function getItemTemplate(ByVal templateID As Integer) As mainCore.cachedItem
            If itemCache.ContainsKey(templateID) = False Then Return New mainCore.cachedItem()
            Return itemCache(templateID)
        End Function
    End Class
    Friend Class recyclerManager
        Private Shared encodingHelper As New HoloENCODING
        Private Shared sessionLength As Integer
        Private Shared sessionExpireLength As Integer
        Private Shared itemMinOwnershipLength As Integer
        Private Shared sessionRewards As New Hashtable
        Friend Shared setupString As String
        Friend Shared Sub Init()
            If HoloRACK.enableRecycler = True Then
                Dim rclrCosts() As Integer = HoloDB.runReadColumn("SELECT rclr_cost FROM system_recycler", 0, Nothing)
                Dim rclrRewards() As Integer = HoloDB.runReadColumn("SELECT rclr_reward FROM system_recycler", 0, Nothing)

                sessionLength = getConfigEntry("recycler_session_length")
                sessionExpireLength = getConfigEntry("recycler_session_expirelength")
                itemMinOwnershipLength = getConfigEntry("recycler_minownertime")

                setupString = "I" & encodingHelper.encodeVL64(itemMinOwnershipLength) & encodingHelper.encodeVL64(sessionLength) & encodingHelper.encodeVL64(sessionExpireLength) & encodingHelper.encodeVL64(rclrCosts.Count)
                For i = 0 To rclrCosts.Count - 1
                    Dim rclrCCT As String = HoloDB.runRead("SELECT name_cct FROM catalogue_items WHERE tid = '" & rclrRewards(i) & "'")
                    If rclrCCT = vbNullString Then Continue For

                    sessionRewards.Add(rclrCosts(i), rclrRewards(i))
                    setupString += encodingHelper.encodeVL64(rclrCosts(i)) & "H" & rclrCCT & Convert.ToChar(2) & "HII" & Convert.ToChar(2)
                Next
            Else
                setupString = "H"
            End If
        End Sub
        Friend Shared ReadOnly Property rewardExists(ByVal itemCount As Integer) As Boolean
            Get
                Return sessionRewards.ContainsKey(itemCount)
            End Get
        End Property
#Region "Session management"
        Friend Shared Sub createSession(ByVal userID As Integer, ByVal itemCount As Integer)
            Dim rewardTemplateID As Integer = sessionRewards(itemCount)
            HoloDB.runQuery("INSERT INTO users_recycler(userid,session_started,session_reward) VALUES ('" & userID & "','" & DateTime.Now.ToString() & "','" & rewardTemplateID & "')")
        End Sub
        Friend Shared Sub dropSession(ByVal userID As Integer, ByVal dropItems As Boolean)
            HoloDB.runQuery("DELETE FROM users_recycler WHERE userid = '" & userID & "' LIMIT 1")
            HoloDB.runQuery("DELETE FROM furniture_recycler WHERE userid = '" & userID & "'")
            If dropItems = True Then
                HoloDB.runQuery("DELETE FROM furniture WHERE ownerid = '" & userID & "' AND roomid = '-2'")
            Else
                HoloDB.runQuery("UPDATE furniture SET roomid = '0' WHERE ownerid = '" & userID & "' AND roomid = '-2'")
            End If
        End Sub
        Friend Shared Sub rewardSession(ByVal userID As Integer)
            HoloDB.runQuery("INSERT INTO furniture(tid,ownerid) VALUES ('" & sessionRewardID(userID) & "','" & userID & "')")
        End Sub
        Friend Shared ReadOnly Property passedMinutes(ByVal userID As Integer) As Integer
            Get
                Dim Span As TimeSpan = DateTime.Now - DateTime.Parse(HoloDB.runRead("SELECT session_started FROM users_recycler WHERE userid = '" & userID & "'"))
                Return Span.TotalMinutes()
            End Get
        End Property
        Friend Shared ReadOnly Property sessionString(ByVal userID As Integer) As String
            Get
                If HoloDB.checkExists("SELECT * FROM users_recycler WHERE userid = '" & userID & "'") = False Then Return "H"

                Dim minutesPassed As Integer = passedMinutes(userID)
                If minutesPassed < sessionLength Then Return "IH" & HoloITEM(sessionRewardID(userID)).cctName & Convert.ToChar(2) & encodingHelper.encodeVL64(sessionLength - minutesPassed)
                If minutesPassed > sessionLength Then Return "JH" & HoloITEM(sessionRewardID(userID)).cctName & Convert.ToChar(2)
                If minutesPassed > sessionExpireLength Then Return "K"

                Return "H"
            End Get
        End Property
        Friend Shared ReadOnly Property sessionExists(ByVal userID As Integer) As Boolean
            Get
                Return HoloDB.checkExists("SELECT userid FROM users_recycler WHERE userid = '" & userID & "'")
            End Get
        End Property
        Friend Shared ReadOnly Property sessionReady(ByVal userID As Integer) As Boolean
            Get
                If sessionExists(userID) = True Then
                    Dim minutesPassed As Integer = passedMinutes(userID)
                    If (minutesPassed > sessionLength) And (minutesPassed < sessionExpireLength) Then Return True
                End If
                Return False
            End Get
        End Property
        Private Shared ReadOnly Property sessionRewardID(ByVal userID As Integer) As Integer
            Get
                Return HoloDB.runRead("SELECT session_reward FROM users_recycler WHERE userid = '" & userID & "'", Nothing)
            End Get
        End Property
#End Region
    End Class
End Class
