Imports System.Threading
Module mainHoloCORE
    '// Core declares
    Public sysChar(255) As Char
    Public HoloDB As New clsHoloDB
    Public HoloSCKMGR As New clsHoloSCKMGR
    Public HoloRACK As New clsHoloRACK
    Public HoloMANAGERS As New clsHoloMANAGERS
    Public HoloENCODING As New HoloENCODING '// Jeax's Habbo encoding class for .NET, featuring B64 and VL64
    Public HoloRANK(7) As clsHoloRANK
    Public HoloMISC As New clsHoloMISC
    Public HoloSTATICMODEL(18) As clsHoloSTATICMODEL

    Public HoloBBGAMELOBBY As clsHoloBBGAMELOBBY

    Public Pathfinder As clsHoloPATHFINDER
    Public HoloITEM(1000) As cachedItemTemplate
    Function filterPacket(ByVal strData As String) As String
        For c = 1 To 12
            'If strData.Contains(sysChar(c)) Then Return vbNullString '// Uncomment this part to kill the connection when a packet contains one of Char 1-12 (it returns an empty packet, thus invalid and this will lead to killConnection)
            strData.Replace(sysChar(c), vbNullString)
        Next
        Return strData
    End Function
    Function safeParse(ByVal Input As String) As Integer
        Try
            Return Integer.Parse(Input)
        Catch
            Return 0
        End Try
    End Function
    Friend Structure cachedItemTemplate
        Sub New(ByVal cctName As String, ByVal typeID As Byte, ByVal Colour As String, ByVal Length As Integer, ByVal Width As Integer, ByVal topH As Double)
            If cctName.Contains("poster ") Then
                Me.cctName = "poster"
                Me.Colour = cctName.Substring(7)
            Else
                Me.cctName = cctName
                Me.Colour = Colour
            End If
            Me.typeID = typeID
            Me.Length = Length
            Me.Width = Width
            Me.topH = topH
            Console.WriteLine("[HOLOCACHE] Cached furniture template [" & cctName & "]")
        End Sub
        Friend typeID As Byte '// The type of the item: 1 = solid, 2 = seat, 3 = bed, 4 = rug
        Friend cctName As String '// The name of the CCT of this item
        Friend Colour As String '// The colour for this item
        Friend Length, Width As Integer '// The length and width of this item
        Friend topH As Double '// The offset of the top of this item, so if you stack ontop of this then it's ontop of the current height + this top height
    End Structure
End Module