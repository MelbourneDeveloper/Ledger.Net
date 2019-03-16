Imports Hardwarewallets.Net.AddressManagement
Imports Hid.Net.Windows

Class MainWindow

    Dim _LedgerManagerBroker As LedgerManagerBroker

    Public Sub New()

        WindowsHidDeviceFactory.Register()

        _LedgerManagerBroker = New LedgerManagerBroker(3000, New DefaultCoinUtility(), New ErrorPromptDelegate(AddressOf Prompt))
        AddHandler _LedgerManagerBroker.LedgerInitialized, AddressOf LedgerManagerBroker_ConnectionEventOccurred
        AddHandler _LedgerManagerBroker.LedgerDisconnected, AddressOf LedgerManagerBroker_ConnectionEventOccurred
        _LedgerManagerBroker.Start()

    End Sub

    Private Sub LedgerManagerBroker_ConnectionEventOccurred(ByVal sender As Object, ByVal e As LedgerManagerConnectionEventArgs)
        ToggleConnected()
    End Sub

    Private Async Sub GetAddressButton_Click(sender As Object, e As RoutedEventArgs) Handles GetAddressButton.Click

        Try
            Dim _LedgerManager As LedgerManager = GetLedger()

            _LedgerManager.SetCoinNumber(195)
            Dim addressPath = AddressPathBase.Parse(Of BIP44AddressPath)(AddressPathBox.Text)
            Dim address = Await _LedgerManager.GetAddressAsync(addressPath, False, False)
            AddressBox.Text = address
            PromptBox.Text = String.Empty

        Catch ex As Exception
            PromptBox.Text = $"Something went wrong.{vbCrLf}{ex.Message}"
        End Try

    End Sub

    Private Sub ToggleConnected()
        Dispatcher.BeginInvoke(ToggleConnectedDelegate())
    End Sub

    Private Function GetLedger() As LedgerManager

        If _LedgerManagerBroker.LedgerManagers.Count = 0 Then Return Nothing

        Return _LedgerManagerBroker.LedgerManagers.First()

    End Function

    Private Function ToggleConnectedDelegate() As [Delegate]
        Return Sub()
                   Dim isConnected As Boolean = Not GetLedger() Is Nothing
                   IsConnectedBox.IsChecked = isConnected
                   GetAddressButton.IsEnabled = isConnected
               End Sub
    End Function

    Private Async Function Prompt(ByVal returnCode As Integer?, ByVal exception As Exception, ByVal member As String) As Task

        Dim ledgerManager As LedgerManager = GetLedger()

        If ledgerManager Is Nothing Then Return

        If returnCode.HasValue Then

            Select Case returnCode.Value
                Case Constants.IncorrectLengthStatusCode
                    PromptBox.Text = $"Please ensure the app {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK"
                Case Constants.SecurityNotValidStatusCode
                    PromptBox.Text = $"It appears that your Ledger pin has not been entered, or no app is open. Please ensure the app  {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK"
                Case Constants.InstructionNotSupportedStatusCode
                    PromptBox.Text = $"The current app is incorrect. Please ensure the app for {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK"
                Case Else
                    PromptBox.Text = $"Something went wrong. Please ensure the app  {ledgerManager.CurrentCoin.App} is open on the Ledger, and press OK"
            End Select
        Else

            If TypeOf exception Is Exception Then

                PromptBox.Text = exception.Message

                Await Task.Delay(1000)

                'The ledger connection is probably broken and will probably never work again, so restart the broker
                ledgerManager.Dispose()
                _LedgerManagerBroker.Start(True)

            End If

        End If

        Await Task.Delay(5000)
    End Function

End Class
