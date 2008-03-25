Module clientFuncLibrary
    Function checkName(ByVal strData As String, ByVal Index As Integer)
        Dim wNamefName As String
        'Check if the name doesn't exist yet and if it's not containing swearwords and/or forbidden names/words
        wName = Mid$(strData, 5, HoloENCODING.decodeB64(Mid$(strData, 3, 2))) 'Get the name from the packet (@j)
        If HoloDB.checkExists("SELECT id FROM users WHERE name = '" & wName & "' LIMIT 1") = True Then 'If there's already a user with that name
            'Send the name is already taken
            Send(Index, "@dPA" & sysChar(1))
        Else
            'Get a wordfiltered copy of the name
            fName = wName
            fName = bFilter(fName, Index)
            If (LCase(fName) <> LCase(wName)) Or checkForbidden(wName) = True Then 'If the original username is different than the filtered name (so it contained a swear word), or it contains a forbidden part
                'Send it's an unacceptable name
                Send(Index, "@dK" & sysChar(1))
            Else 'Let the new-user proceed in registration
                Send(Index, "@dH" & sysChar(1))
            End If
        End If
    End Function
End Module
