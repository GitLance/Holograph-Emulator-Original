Public Class clsHoloRACK
    Friend configFileLocation As String
    Friend gameSocket_Port, gameSocket_maxConnections As Integer
    Friend musSocket_Port, musSocket_maxConnections As Integer, musSocket_Host As String

    Friend onlinePeak As Integer
    Friend acceptedConnections As Integer
    Friend wordFilter_Enabled As Boolean
    Friend wordFilter_Words() As String
    Friend wordFilter_Replacement As String
    Friend welcMessage As String
    Friend Console_OnHotelView As String
    Friend Chat_Animations As Boolean '// To show facial emotions, talking animation and head tilts during chat
    Friend roomModels As New Hashtable
    Friend cataloguePages As Hashtable
    Friend Structure cachedCataloguePage
        Friend displayName As String
        Friend strPage As String
    End Structure
End Class
