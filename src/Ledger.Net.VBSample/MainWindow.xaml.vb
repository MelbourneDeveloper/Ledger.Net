Imports Hid.Net.Windows

Class MainWindow

    Dim _LedgerManagerBroker As LedgerManagerBroker

    Private Function GetLedger() As LedgerManager

        If _LedgerManagerBroker.LedgerManagers.Count = 0 Then Return Nothing

        Return _LedgerManagerBroker.LedgerManagers.First()

    End Function

    Public Sub New()

        WindowsHidDeviceFactory.Register()

        _LedgerManagerBroker = New LedgerManagerBroker(3000, New DefaultCoinUtility(), New ErrorPromptDelegate(AddressOf Prompt))
        AddHandler _LedgerManagerBroker.LedgerInitialized, AddressOf LedgerManagerBroker_LedgerInitialized
        _LedgerManagerBroker.Start()

    End Sub

    Private Sub LedgerManagerBroker_LedgerInitialized(ByVal sender As Object, ByVal e As LedgerManagerConnectionEventArgs)

        Dispatcher.BeginInvoke(Sub() IsConnectedBox.IsChecked = Not GetLedger() Is Nothing)


    End Sub

    Private Sub GetAddressButton_Click(sender As Object, e As RoutedEventArgs) Handles GetAddressButton.Click

    End Sub

    Private Async Function Prompt(ByVal returnCode As Integer?, ByVal exception As Exception, ByVal member As String) As Task

        Dim ledgerManager As LedgerManager = GetLedger()

        If ledgerManager Is Nothing Then Return

        If returnCode.HasValue Then

            Select Case returnCode.Value
                Case Constants.IncorrectLengthStatusCode
                    Debug.WriteLine($"Please ensure the app {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK")
                Case Constants.SecurityNotValidStatusCode
                    Debug.WriteLine($"It appears that your Ledger pin has not been entered, or no app is open. Please ensure the app  {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK")
                Case Constants.InstructionNotSupportedStatusCode
                    Debug.WriteLine($"The current app is incorrect. Please ensure the app for {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK")
                Case Else
                    Debug.WriteLine($"Something went wrong. Please ensure the app  {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK")
            End Select
        Else

            ''TODO: Deal with IO errors

            'If TypeOf exception Is IOException Then
            '    Await Task.Delay(3000)
            '    Await _LedgerManager.LedgerHidDevice.InitializeAsync()
            'End If
        End If

        Await Task.Delay(5000)
    End Function

End Class
