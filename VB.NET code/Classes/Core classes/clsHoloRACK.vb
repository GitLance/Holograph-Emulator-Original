Public Class clsHoloRACK
    Friend configFileLocation As String
    Friend sPort As Integer
    Friend maxConnections As Integer
    Friend freeScks As String
    Friend onlineCount As Integer
    Friend onlinePeak As Integer
    Friend acceptedConnections As Integer
    Friend ssoLogin As Boolean
    Friend wordFilter_Enabled As Boolean
    Friend wordFilter_Words() As String
    Friend wordFilter_Replacement As String
    Friend roomModels As New Hashtable
    Friend catalogueIndex As String
    Friend catalogueIndexAdmin As String
    Friend welcMessage As String
    Friend petSound(0 To 2, 0 To 8) As String
    Friend msgSubscriptionExpired As String = "Hey %name%, it seems your subscription to Club has elapsed!\rThis means you have lost your Club badges and your look is set to a default look.\rOfcourse you'll keep your special rooms and your presents!\r\rWe thank you for being a subscriber to Club, btw, did you considered purchasing a new subscription?\rYou're always welcome!\rNow gtfo you little cunt, you are supposed to drool at pixel furnis!\r\rHotel Leets"
    Friend Console_OnHotelView As String
    Friend Chat_Animations As Boolean '// To show facial emotions, talking animation and head tilts during chat
    Friend cataloguePages As Hashtable
    Friend Structure cachedCataloguePage
        Friend displayName As String
        Friend strPage As String
    End Structure
End Class
