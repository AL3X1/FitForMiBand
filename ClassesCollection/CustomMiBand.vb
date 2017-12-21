Imports System.Threading
Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Bluetooth.GenericAttributeProfile
Imports Windows.Security.Cryptography.Core
Imports Windows.Storage.Streams

Public Class CustomMiBand
    Implements IDisposable
    Implements INotifyPropertyChanged

    Public Enum AuthorizationStatus
        Failed = 0
        Success = 1
    End Enum

    Private _DeviceId As String
    Public Property DeviceId() As String
        Get
            Return _DeviceId
        End Get
        Set(ByVal value As String)
            _DeviceId = value
        End Set
    End Property

    Private _Authorized As Boolean
    Public Property Authorized() As Boolean
        Get
            Return _Authorized
        End Get
        Set(ByVal value As Boolean)
            _Authorized = value
        End Set
    End Property

    Private _DisplayItems As List(Of CustomMiBandResult)
    Public Property DisplayItems() As List(Of CustomMiBandResult)
        Get
            Return _DisplayItems
        End Get
        Set(ByVal value As List(Of CustomMiBandResult))
            _DisplayItems = value
        End Set
    End Property

    Private _BatteryResult As BatteryResult
    Public Property BatteryResult() As BatteryResult
        Get
            Return _BatteryResult
        End Get
        Set(ByVal value As BatteryResult)
            _BatteryResult = value
        End Set
    End Property

    Private _StepResult As StepResult
    Public Property StepResult() As StepResult
        Get
            Return _StepResult
        End Get
        Set(ByVal value As StepResult)
            _StepResult = value
        End Set
    End Property

    Private WithEvents _HeartResult As HeartResult
    Public Property HeartResult() As HeartResult
        Get
            Return _HeartResult
        End Get
        Set(ByVal value As HeartResult)
            _HeartResult = value
        End Set
    End Property

    Private _NotificationResult As NotificationResult
    Public Property NotificationResult() As NotificationResult
        Get
            Return _NotificationResult
        End Get
        Set(ByVal value As NotificationResult)
            _NotificationResult = value
        End Set
    End Property

    Private Device As BluetoothLEDevice
    Private GattServices As GattDeviceServicesResult
    Private WithEvents GattCharacteristic As GattCharacteristic
    Private DeviceSelector As String = BluetoothLEDevice.GetDeviceSelector
    Private LocalSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings
    Private disposedValue As Boolean
    Private _updatemode As Boolean = False

    Public Delegate Sub AuthorizationCompletedEventHandler(sender As Object, e As AuthorizationStatus)

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Public Event InitializationCompleted As AuthorizationCompletedEventHandler

    Private ReadOnly WaitEvent As New ManualResetEvent(False)

    Public Sub New()
        _DeviceId = ""
        _DisplayItems = New List(Of CustomMiBandResult)

        If LocalSettings.Values("DeviceId") IsNot Nothing Then
            _DeviceId = LocalSettings.Values("DeviceId").ToString
            _BatteryResult = New BatteryResult
            _StepResult = New StepResult
            _HeartResult = New HeartResult
            _NotificationResult = New NotificationResult

            If _BatteryResult.Initialize Then
                _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Battery, _BatteryResult.Percentage.ToString("N0"), Helpers.TimeSpanToText(_BatteryResult.LastChargingDate)))
            End If

            If _StepResult.Initialize Then
                _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Steps, _StepResult.TotalSteps.ToString, _StepResult.StepsReachedInPercent))
                _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Distance, _StepResult.TotalDistance.ToString, ""))
                _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Calories, _StepResult.TotalCals.ToString, ""))
            End If

            If _HeartResult.Initialize Then
                _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Heartrate, _HeartResult.HeartRate.ToString("N0"), _HeartResult.Title))
            End If

            NotificationResult.Initialize()
            _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Notifications, _NotificationResult.Requests.Count.ToString, "Active"))

        Else
            LocalSettings.Values("Setting_8") = True
            _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Welcome, "Welcome", "Tap to setup your device and profile"))
        End If
    End Sub

    Public Async Function ConnectWithAuth() As Task(Of Boolean)
        Try
            Device = Await BluetoothLEDevice.FromIdAsync(_DeviceId)
            If Device IsNot Nothing Then
                If Device.ConnectionStatus = BluetoothConnectionStatus.Disconnected Then
                    ' Get all services & Authenticate
                    GattServices = Await Device.GetGattServicesAsync(BluetoothCacheMode.Uncached)
                    If GattServices.Status = GattCommunicationStatus.Success Then
                        Return Await AuthenticateAppOnDevice()
                    Else
                        ' Device not reachable
                        Return False
                    End If
                Else
                    ' Get all services & Authenticate
                    GattServices = Await Device.GetGattServicesAsync(BluetoothCacheMode.Uncached)
                    If GattServices.Status = GattCommunicationStatus.Success Then
                        Return True
                    Else
                        Return False
                    End If
                End If
            Else
                ' not paired anymore or bluetooth dead
                Return False
            End If

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
            Return False
        End Try
    End Function

    Private Async Function Connect() As Task(Of Boolean)
        Try
            Device = Await BluetoothLEDevice.FromIdAsync(_DeviceId)
            If Device IsNot Nothing Then
                If Device.ConnectionStatus = BluetoothConnectionStatus.Disconnected Then
                    ' Get all services
                    GattServices = Await Device.GetGattServicesAsync(BluetoothCacheMode.Uncached)
                    If GattServices.Status = GattCommunicationStatus.Success Then
                        Return True
                    Else
                        ' Device not reachable
                        Return False
                    End If
                Else
                    ' Get all services 
                    GattServices = Await Device.GetGattServicesAsync(BluetoothCacheMode.Uncached)
                    If GattServices.Status = GattCommunicationStatus.Success Then
                        Return True
                    Else
                        ' Device not reachable
                        Return False
                    End If
                End If
            Else
                ' not paired anymore or bluetooth dead
                Return False
            End If

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
            Return False
        End Try
    End Function

    Private Sub Disconnect()
        If GattCharacteristic IsNot Nothing Then
            If GattCharacteristic.Service IsNot Nothing Then
                GattCharacteristic.Service.Dispose()
            End If
            GattCharacteristic = Nothing
        End If
        GattServices = Nothing
        If Device IsNot Nothing Then Device.Dispose()
        Device = Nothing
    End Sub

    Public Async Function AuthenticateAppOnDevice() As Task(Of Boolean)
        Try
            Debug.WriteLine($"Request Handler for ValueChanged")
            GattCharacteristic = Await GetCharacteristic(GetService(CustomBluetoothProfile.Authentication.service), CustomBluetoothProfile.Authentication.authCharacteristic)

            If Await GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) = GattCommunicationStatus.Success Then
                Debug.WriteLine($"Requested Handler added")
            Else
                Debug.WriteLine($"Requested Handler not permitted")
            End If

            If CBool(LocalSettings.Values("IsAuthenticationNeeded")) = True Then
                ' Build payload for authentication
                Dim sendKey As New List(Of Byte)
                sendKey.Add(CustomBluetoothProfile.Authentication.AUTH_SEND_KEY)
                sendKey.Add(CustomBluetoothProfile.Authentication.AUTH_BYTE)
                sendKey.AddRange(GetSecretKey())

                If Await GattCharacteristic.WriteValueAsync(sendKey.ToArray.AsBuffer) = GattCommunicationStatus.Success Then
                    Debug.WriteLine($"Authentication Level 1 successfully reached")
                Else
                    Debug.WriteLine($"Authentication Level 1 FAILED")
                    Return False
                End If
            Else
                Debug.WriteLine($"Authentication Level 1 successfully reached")

                If Await GattCharacteristic.WriteValueAsync(RequestAuthNumber.AsBuffer) = GattCommunicationStatus.Success Then
                    Debug.WriteLine($"Authentication Level 2 successfully reached")
                    Debug.WriteLine($"Request Auth Number successfully")
                Else
                    Debug.WriteLine($"Authentication Level 2 FAILED")
                    Debug.WriteLine($"Request Auth Number FAILED")
                    Return False
                End If
            End If

            Return WaitEvent.WaitOne(30000)

        Catch ex As Exception
            Return False
        Finally
            WaitEvent.Reset()
        End Try
    End Function

    Private Async Sub gattCharacteristic_ValueChanged(sender As GattCharacteristic, args As GattValueChangedEventArgs) Handles GattCharacteristic.ValueChanged
        If sender.Uuid = CustomBluetoothProfile.Authentication.authCharacteristic Then
            Debug.WriteLine($"Characteristic value received: {Text.Encoding.ASCII.GetString(args.CharacteristicValue.ToArray)}")

            ' Check response
            If args.CharacteristicValue.ToArray.ToList(0) = CustomBluetoothProfile.Authentication.AUTH_RESPONSE And
                    args.CharacteristicValue.ToArray.ToList(1) = CustomBluetoothProfile.Authentication.AUTH_SEND_KEY And
                    args.CharacteristicValue.ToArray.ToList(2) = CustomBluetoothProfile.Authentication.AUTH_SUCCESS Then

                Dim doCharResult = Await sender.WriteValueAsync(RequestAuthNumber.AsBuffer)
                If doCharResult = GattCommunicationStatus.Success Then
                    Debug.WriteLine($"Authentication Level 2 successfully reached")
                    Debug.WriteLine($"SUCCESS: Request Random Auth Number")
                Else
                    Debug.WriteLine($"Authentication Level 2 FAILED")
                    Debug.WriteLine($"FAILED: Request Random Auth Number")
                    OnInitializationCompleted(AuthorizationStatus.Failed)
                End If

            ElseIf args.CharacteristicValue.ToArray.ToList(0) = CustomBluetoothProfile.Authentication.AUTH_RESPONSE And
                    args.CharacteristicValue.ToArray.ToList(1) = CustomBluetoothProfile.Authentication.AUTH_REQUEST_RANDOM_AUTH_NUMBER And
                    args.CharacteristicValue.ToArray.ToList(2) = CustomBluetoothProfile.Authentication.AUTH_SUCCESS Then

                ' Send encryted random key to band
                Dim doCharResult = Await sender.WriteValueAsync(SendEncryptedRandomKey(args.CharacteristicValue.ToArray).AsBuffer)
                If doCharResult = GattCommunicationStatus.Success Then
                    Debug.WriteLine($"Authentication Level 3 successfully reached")
                    Debug.WriteLine($"SUCCESS: Send encrypted random key to band")
                Else
                    Debug.WriteLine($"Authentication Level 3 FAILED")
                    Debug.WriteLine($"FAILED: Send encrypted random key to band")
                    OnInitializationCompleted(AuthorizationStatus.Failed)
                End If

            ElseIf args.CharacteristicValue.ToArray.ToList(0) = CustomBluetoothProfile.Authentication.AUTH_RESPONSE And
                    args.CharacteristicValue.ToArray.ToList(1) = CustomBluetoothProfile.Authentication.AUTH_SEND_ENCRYPTED_AUTH_NUMBER And
                    args.CharacteristicValue.ToArray.ToList(2) = CustomBluetoothProfile.Authentication.AUTH_SUCCESS Then

                ' Authenticated, now initialize phase 2
                Debug.WriteLine($"WE ARE AUTHENTICATED ON MI BAND 2")
                LocalSettings.Values("IsAuthenticationNeeded") = False
                LocalSettings.Values("IsAuthenticated") = True

                ' Raise Event
                OnInitializationCompleted(AuthorizationStatus.Success)

            ElseIf args.CharacteristicValue.ToArray.ToList(0) = CustomBluetoothProfile.Authentication.AUTH_RESPONSE And
                    args.CharacteristicValue.ToArray.ToList(1) = CustomBluetoothProfile.Authentication.AUTH_SEND_ENCRYPTED_AUTH_NUMBER And
                    args.CharacteristicValue.ToArray.ToList(2) = CustomBluetoothProfile.Authentication.AUTH_FAIL Then

                Debug.WriteLine($"Authentication for Phase 2 FAILED")
                Debug.WriteLine($"Authentication State set to AuthenticationNeeded")
                LocalSettings.Values("IsAuthenticationNeeded") = True
                LocalSettings.Values("IsAuthenticated") = False

                OnInitializationCompleted(AuthorizationStatus.Failed)

            Else
                Debug.WriteLine($"Unhandled Value in Characteristic {sender.Uuid}")
                'No User action
                LocalSettings.Values("IsAuthenticationNeeded") = True
                LocalSettings.Values("IsAuthenticated") = False

                OnInitializationCompleted(AuthorizationStatus.Failed)
            End If
        Else
            Debug.WriteLine($"Unhandled Characteristic {sender.Uuid}")

            OnInitializationCompleted(AuthorizationStatus.Failed)
        End If
    End Sub


    Private Function RequestAuthNumber() As Byte()
        Dim AuthNumber As New List(Of Byte)
        Try
            AuthNumber.Add(CustomBluetoothProfile.Authentication.AUTH_REQUEST_RANDOM_AUTH_NUMBER)
            AuthNumber.Add(CustomBluetoothProfile.Authentication.AUTH_BYTE)

            Return AuthNumber.ToArray

        Catch ex As Exception
            Return AuthNumber.ToArray
        End Try
    End Function

    Private Function GetSecretKey() As Byte()
        Dim SecretKey As New List(Of Byte)
        Try
            SecretKey.Add(48) '0x30
            SecretKey.Add(49) '0x31
            SecretKey.Add(50) '0x32
            SecretKey.Add(51) '0x33
            SecretKey.Add(52) '0x34
            SecretKey.Add(53) '0x35
            SecretKey.Add(54) '0x36
            SecretKey.Add(55) '0x37
            SecretKey.Add(56) '0x38
            SecretKey.Add(57) '0x39
            SecretKey.Add(64) '0x40
            SecretKey.Add(65) '0x41
            SecretKey.Add(66) '0x42
            SecretKey.Add(67) '0x43
            SecretKey.Add(68) '0x44
            SecretKey.Add(69) '0x45

            Return SecretKey.ToArray

        Catch ex As Exception
            Return SecretKey.ToArray
        End Try
    End Function

    Private Function SendEncryptedRandomKey(ByVal ResponseValue As Byte()) As Byte()
        Dim RandomKey As New List(Of Byte)
        Dim RelevantResponsePart As New List(Of Byte)
        Try
            ' Take Part of ResponseValue 3 to 19 (2 to 18)
            For i = 0 To ResponseValue.Count - 1
                If i >= 3 Then ' 19 is highest
                    RelevantResponsePart.Add(ResponseValue(i))
                End If
            Next

            RandomKey.Add(CustomBluetoothProfile.Authentication.AUTH_SEND_ENCRYPTED_AUTH_NUMBER)
            RandomKey.Add(CustomBluetoothProfile.Authentication.AUTH_BYTE)
            RandomKey.AddRange(EncryptToAES(RelevantResponsePart.ToArray))

            Return RandomKey.ToArray

        Catch ex As Exception
            Return RandomKey.ToArray
        End Try
    End Function

    Private Function EncryptToAES(ByVal ToBeEncryptedBytes As Byte()) As Byte()
        Dim BufferEncrypt As IBuffer = Nothing
        Dim AesKey As String = Nothing
        Dim ckey As CryptographicKey = Nothing
        Dim key As IBuffer = Nothing
        Dim provider As SymmetricKeyAlgorithmProvider
        Try
            ' Get Key secret
            AesKey = Convert.ToBase64String(GetSecretKey)

            ' Create aes Key
            key = Convert.FromBase64String(AesKey).AsBuffer
            provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbc)
            ckey = provider.CreateSymmetricKey(key)

            ' encrypt bytes
            BufferEncrypt = CryptographicEngine.Encrypt(ckey, ToBeEncryptedBytes.AsBuffer, Nothing)

            Return BufferEncrypt.ToArray

        Catch ex As Exception
            Return Nothing
        Finally
            BufferEncrypt = Nothing
            ckey = Nothing
            AesKey = Nothing
            key = Nothing
            provider = Nothing
        End Try
    End Function

    Public Async Function GetCharacteristic(ByVal Service As GattDeviceService, ByVal UUID As Guid) As Task(Of GattCharacteristic)
        Dim CharacteristicsResult = Await Service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached)
        Return CharacteristicsResult.Characteristics.Where(Function(x) x.Uuid = UUID).FirstOrDefault
    End Function

    Public Function GetService(ByVal UUID As Guid) As GattDeviceService
        Return GattServices.Services.Where(Function(x) x.Uuid = UUID).FirstOrDefault
    End Function

    Public Async Function SetNewAlert(ByVal mApp As NotificationRequest, ByVal mTitle As String, ByVal mText As String) As Task(Of Boolean)
        Dim message As New List(Of Byte)
        Try
            If Await ConnectWithAuth() Then

                ' Prepare Title
                If mTitle <> String.Empty And mApp.DisplayMethod = NotificationRequest.DisplayMethods.MessageTitle Then
                    If mTitle.Length > 18 Then
                        mTitle = mTitle.Substring(0, 15) & "..."
                    End If

                    ' Send Title to device with vibration
                    message.Clear()
                    message.AddRange(New Byte() {5, 1}) ' SMS-Icon with Text
                    message.AddRange(System.Text.Encoding.ASCII.GetBytes(mTitle))
                    Await SendMessage(message.ToArray)

                    '' Prepare Text
                    'message.Clear()
                    'If mText <> String.Empty Then
                    '    If mText.Length > 18 Then
                    '        ' Send multipe Text without vibration
                    '        For Each mTextpart In Helpers.SplitByLength(mText, 18, 3)
                    '            message.Clear()
                    '            message.AddRange(New Byte() {5, 1}) ' 5 = SMS, 3 = Phone
                    '            message.AddRange(System.Text.Encoding.UTF8.GetBytes(mTextpart))
                    '            Await SendMessage(message.ToArray)
                    '        Next
                    '    Else
                    '        ' Send single Text without vibration ?
                    '        message.Clear()
                    '        message.AddRange(New Byte() {5, 1})
                    '        message.AddRange(System.Text.Encoding.UTF8.GetBytes(mText))
                    '        Await SendMessage(message.ToArray)
                    '    End If
                    'End If
                Else
                    ' Icon Only Notification with real Icon if possible
                    message.Clear()

                    Select Case mApp.Id
                        Case "5319275A.WhatsApp_cv1g1gvanyjgm!x218a0ebby1585y4c7eya9ecy054cf4569a79x"
                            message.AddRange(New Byte() {CustomBluetoothProfile.NofiticationService.ALERT_LEVEL_CUSTOM, 1, CustomBluetoothProfile.NofiticationService.ICON_WHATSAPP})
                        Case "KALENDER"
                            message.AddRange(New Byte() {CustomBluetoothProfile.NofiticationService.ALERT_LEVEL_CUSTOM, 1, CustomBluetoothProfile.NofiticationService.ICON_CALENDAR})
                        Case "FACEBOOK"
                            message.AddRange(New Byte() {CustomBluetoothProfile.NofiticationService.ALERT_LEVEL_CUSTOM, 1, CustomBluetoothProfile.NofiticationService.ICON_FB})
                        Case "TWITTER"
                            message.AddRange(New Byte() {CustomBluetoothProfile.NofiticationService.ALERT_LEVEL_CUSTOM, 1, CustomBluetoothProfile.NofiticationService.ICON_TWITTER})
                        Case Else
                            ' SMS-Icon with App-Name
                            If mApp.DisplayName.Length > 18 Then
                                mApp.DisplayName = mApp.DisplayName.Substring(0, 15) & "..."
                            End If
                            message.AddRange(New Byte() {5, 1}) ' SMS-Icon with Text
                            message.AddRange(System.Text.Encoding.ASCII.GetBytes(mApp.DisplayName))
                    End Select

                    Await SendMessage(message.ToArray)
                End If
            End If

        Catch ex As Exception

        Finally
            message = Nothing

            Disconnect()
        End Try

        Return True
    End Function

    Private Async Function SendMessage(ByVal mMessage As Byte()) As Task
        Debug.WriteLine($"Sending {String.Join(", ", mMessage)} as NewAlert")

        Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.NofiticationService.service), CustomBluetoothProfile.NofiticationService.newAlertCharacteristic)
        Dim r = doChar.WriteValueAsync(mMessage.AsBuffer)

        Await Task.Delay(4500)
    End Function

    Public Async Function SetAlertLevel(ByVal level As Byte) As Task(Of Boolean)
        Try
            If Await ConnectWithAuth() Then
                Dim bytes = New Byte() {level}
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.AlertLevel.service), CustomBluetoothProfile.AlertLevel.alertLevelCharacteristic)
                Dim r = doChar.WriteValueAsync(bytes.AsBuffer)

            End If

        Catch ex As Exception

        Finally
            Disconnect()
        End Try

        Return True
    End Function

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                GattCharacteristic = Nothing
                GattServices = Nothing
                If Device IsNot Nothing Then Device.Dispose()
                Device = Nothing
            End If
        End If
        disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(disposing As Boolean) weiter oben ein.
        Dispose(True)
        ' GC.SuppressFinalize(Me)
    End Sub

    Private Sub OnInitializationCompleted(ByVal mStatus As AuthorizationStatus)
        Try
            WaitEvent.Set()

            RaiseEvent InitializationCompleted(Me, mStatus)

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
        Finally

        End Try
    End Sub

    Public Async Function UpdateOperations() As Task(Of Boolean)
        Try
            If Await ConnectWithAuth() Then

                _DisplayItems.Clear()

                If Await _StepResult.GetSteps(Await GetCharacteristic(GetService(StepResult.SERVICE), StepResult.REALTIME_STEPS)) = GattCommunicationStatus.Success Then
                    _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Steps, _StepResult.TotalSteps.ToString, _StepResult.StepsReachedInPercent))
                    _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Distance, _StepResult.TotalDistance.ToString, ""))
                    _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Calories, _StepResult.TotalCals.ToString, ""))
                End If

                If Await _BatteryResult.GetBatteryInfo(Await GetCharacteristic(GetService(BatteryResult.SERVICE), BatteryResult.BATTERY_INFO)) = GattCommunicationStatus.Success Then
                    _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Battery, _BatteryResult.Percentage.ToString("N0"), Helpers.TimeSpanToText(_BatteryResult.LastChargingDate)))
                End If

                _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Heartrate, _HeartResult.HeartRate.ToString("N0"), HeartResult.Title))

                NotificationResult.Initialize()
                _DisplayItems.Add(New CustomMiBandResult(CustomMiBandResult.BandOperation.Notifications, _NotificationResult.Requests.Count.ToString, "Active"))

                OnPropertyChanged("DisplayItems")

                Return True

            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        Finally
            Disconnect()
        End Try
    End Function

    Private Function IsOperationEnabled(ByVal _Operation As CustomMiBandResult.BandOperation) As Boolean
        If LocalSettings.Values(String.Format("Setting_{0}", CInt(_Operation))) IsNot Nothing Then
            Return CBool(LocalSettings.Values(String.Format("Setting_{0}", CInt(_Operation))))
        Else
            Return False
        End If
    End Function

    Private Async Function GetClockStatus() As Task(Of ClockResult)
        Try
            Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.timeCharacterisic)
            Dim resultRead = Await doChar.ReadValueAsync(BluetoothCacheMode.Uncached)
            If resultRead.Status = GattCommunicationStatus.Success Then

                Dim resultBytes = resultRead.Value.ToArray
                '1 = 07 = 201 + 07 = 2017 (year)
                '2 = month
                '3 = day
                '4 = hour
                '5 = minutes
                '6 = seconds

                Return New ClockResult(CDate(String.Format("{0}.{1}.201{2} {3}:{4}:{5}", resultBytes(3), resultBytes(2), resultBytes(1), resultBytes(4), resultBytes(5), resultBytes(6))))
            End If

            Return Nothing

        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Async Function GetDeviceName() As Task(Of Boolean)
        Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Generic.service), CustomBluetoothProfile.Generic.devicenameCharacteristic)
        Dim resultRead = Await doChar.ReadValueAsync
        If resultRead.Status = GattCommunicationStatus.Success Then
            LocalSettings.Values("DeviceName") = Text.Encoding.UTF8.GetString(resultRead.Value.ToArray)
        End If

        Return True

    End Function

    Public Async Function GetSoftwareRevision() As Task(Of Boolean)
        Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Information.service), CustomBluetoothProfile.Information.softwareCharacteristic)
        Dim resultRead = Await doChar.ReadValueAsync
        If resultRead.Status = GattCommunicationStatus.Success Then
            LocalSettings.Values("SoftwareRevision") = Text.Encoding.UTF8.GetString(resultRead.Value.ToArray)
        End If

        Return True

    End Function

    Public Async Function setActivateDisplayOnLiftWrist() As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setActivateDisplayOnLiftWrist")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If Convert.ToBoolean(LocalSettings.Values("IsDisplayOnLiftWristEnabled")) Then
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_ENABLE_DISPLAY_ON_LIFT_WRIST
                Else
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DISABLE_DISPLAY_ON_LIFT_WRIST
                End If
                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    Return True
                End If
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setDateDisplay() As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setDateDisplay")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If Convert.ToBoolean(LocalSettings.Values("IsDateEnabled")) Then
                    _bytes = CustomBluetoothProfile.Basic.DATEFORMAT_DATE_TIME
                Else
                    _bytes = CustomBluetoothProfile.Basic.DATEFORMAT_TIME
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    Return True
                End If
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setFitnessGoal(ByVal IsEnabled As Boolean) As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setFitnessGoal")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If IsEnabled Then
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_ENABLE_GOAL_NOTIFICATION
                Else
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DISABLE_GOAL_NOTIFICATION
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    LocalSettings.Values("IsGoalNotificationEnabled") = IsEnabled
                End If
            End If

            Return True
        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setDisplayItems() As Task(Of Boolean)
        Dim _bytes As New List(Of Byte)
        Try
            Debug.WriteLine($"BAND: setDisplayItems")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                _bytes.AddRange(CustomBluetoothProfile.Basic.COMMAND_CHANGE_SCREENS)

                If Convert.ToBoolean(LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Steps)))) Then _bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) = (_bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) Or CustomBluetoothProfile.Basic.DISPLAY_ITEM_BIT_STEPS)
                If Convert.ToBoolean(LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Distance)))) Then _bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) = (_bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) Or CustomBluetoothProfile.Basic.DISPLAY_ITEM_BIT_DISTANCE)
                If Convert.ToBoolean(LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Calories)))) Then _bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) = (_bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) Or CustomBluetoothProfile.Basic.DISPLAY_ITEM_BIT_CALORIES)
                If Convert.ToBoolean(LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Heartrate)))) Then _bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) = (_bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) Or CustomBluetoothProfile.Basic.DISPLAY_ITEM_BIT_HEART_RATE)
                If Convert.ToBoolean(LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Battery)))) Then _bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) = (_bytes(CustomBluetoothProfile.Basic.SCREEN_CHANGE_BYTE) Or CustomBluetoothProfile.Basic.DISPLAY_ITEM_BIT_BATTERY)

                If Await doChar.WriteValueAsync(_bytes.ToArray.AsBuffer) = GattCommunicationStatus.Success Then
                    Return True
                End If
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setUserInfo(ByVal mAlias As String, ByVal mDateOfBirth As Date, ByVal mGender As String, ByVal mHeight As Integer, ByVal mWeight As Integer) As Task(Of Boolean)
        Dim _bytes As New List(Of Byte)
        Try
            Debug.WriteLine($"BAND: setUserInfo")

            If Await Connect() Then

                Dim userid As Integer = Integer.Parse("4711") ' is originaly an account number
                Dim gender As Integer = 2
                If mGender = "Male" Then
                    gender = 0
                Else
                    gender = 1
                End If

                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.usersettingsCharacteristic)

                _bytes.Add(CustomBluetoothProfile.Basic.COMMAND_SET_USERINFO)
                _bytes.Add(0)
                _bytes.Add(0)
                _bytes.Add(CByte((mDateOfBirth.Year And 255))) 'year
                _bytes.Add(CByte((mDateOfBirth.Year >> 8) And 255)) 'year
                _bytes.Add(CByte(mDateOfBirth.Month)) 'Month
                _bytes.Add(CByte(mDateOfBirth.Day)) ' Day
                _bytes.Add(CByte(gender)) 'Gender ( 2 = Others, 0 = Male, 1 = Female)
                _bytes.Add(CByte((mHeight And 255))) 'height
                _bytes.Add(CByte(((mHeight >> 8) And 255))) 'height
                _bytes.Add(CByte(((mWeight * 200) And 255))) 'Weight
                _bytes.Add(CByte((((mWeight * 200 >> 8) And 255)))) 'Weight
                _bytes.Add(CByte((userid And 255))) 'userid
                _bytes.Add(CByte(((userid >> 8) And 255))) 'userid
                _bytes.Add(CByte(((userid >> 16) And 255))) 'userid
                _bytes.Add(CByte(((userid >> 24) And 255))) 'userid


                If Await doChar.WriteValueAsync(_bytes.ToArray.AsBuffer) = GattCommunicationStatus.Success Then
                    ' Yippeeeh
                End If
            End If

            Return True
        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setWearLocation() As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setWearLocation")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.usersettingsCharacteristic)

                If CBool(LocalSettings.Values("IsWearLocationRightEnabled")) Then
                    _bytes = CustomBluetoothProfile.Basic.WEAR_LOCATION_RIGHT_WRIST
                Else
                    _bytes = CustomBluetoothProfile.Basic.WEAR_LOCATION_LEFT_WRIST
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    Return True
                End If
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setDoNotDisturb() As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setDoNotDisturb")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If Convert.ToBoolean(LocalSettings.Values("IsDndEnabled")) Then
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DO_NOT_DISTURB_AUTOMATIC
                Else
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DO_NOT_DISTURB_OFF
                End If
                'Select Case Modus
                '    Case 0
                '        _bytes = CustomBluetoothProfile.Basic.COMMAND_DO_NOT_DISTURB_OFF
                '    Case 1
                '        _bytes = CustomBluetoothProfile.Basic.COMMAND_DO_NOT_DISTURB_AUTOMATIC
                '    Case 2
                '        _bytes = CustomBluetoothProfile.Basic.COMMAND_DO_NOT_DISTURB_SCHEDULED

                '        'ToDo: Crazy Stuff


                '        'https://github.com/Freeyourgadget/Gadgetbridge/blob/c325ba1a22e60ef6ab6624b648d6f41f36f2417a/app/src/main/java/nodomain/freeyourgadget/gadgetbridge/service/devices/miband2/MiBand2Support.java
                '        '                        Byte[] data = MiBand2Service.COMMAND_DO_NOT_DISTURB_SCHEDULED.clone();

                '        '                Calendar calendar = GregorianCalendar.getInstance();

                '        '                Date start = HuamiCoordinator.getDoNotDisturbStart();
                '        '                calendar.setTime(start);
                '        '                Data[MiBand2Service.DND_BYTE_START_HOURS] = (Byte) calendar.Get(Calendar.HOUR_OF_DAY);
                '        '                Data[MiBand2Service.DND_BYTE_START_MINUTES] = (Byte) calendar.Get(Calendar.MINUTE);

                '        '                Date end = HuamiCoordinator.getDoNotDisturbEnd();
                '        '                calendar.setTime(end);
                '        '                Data[MiBand2Service.DND_BYTE_END_HOURS] = (Byte) calendar.Get(Calendar.HOUR_OF_DAY);
                '        'Data[MiBand2Service.DND_BYTE_END_MINUTES] = (Byte) calendar.Get(Calendar.MINUTE);

                'End Select

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                        Return True
                    End If
                End If

                Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setMetricSystem(ByVal IsMilesEnabled As Boolean) As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setMetricSystem")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If IsMilesEnabled Then
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DISTANCE_UNIT_IMPERIAL
                Else
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DISTANCE_UNIT_METRIC
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    LocalSettings.Values("IsMilesEnabled") = IsMilesEnabled
                End If
            End If

            Return True
        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setGoalNotification() As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setGoalNotification")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If Convert.ToBoolean(LocalSettings.Values("IsGoalNotificationEnabled")) Then
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_ENABLE_GOAL_NOTIFICATION
                Else
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DISABLE_GOAL_NOTIFICATION
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    Return True
                End If
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setRotateWristToSwitchInfo() As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setRotateWristToSwitchInfo")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If Convert.ToBoolean(LocalSettings.Values("IsRotateWristToSwitchInfoEnabled")) Then
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_ENABLE_ROTATE_WRIST_TO_SWITCH_INFO
                Else
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DISABLE_ROTATE_WRIST_TO_SWITCH_INFO
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    Return True
                End If
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setDisplayCallerId(ByVal IsEnabled As Boolean) As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setDisplayCallerId")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If IsEnabled Then
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_ENABLE_DISPLAY_CALLER
                Else
                    _bytes = CustomBluetoothProfile.Basic.COMMAND_DISABLE_DISPLAY_CALLER
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    LocalSettings.Values("IsDisplayCallerIdEnabled") = IsEnabled
                End If
            End If

            Return True
        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Public Async Function setTimeFormatDisplay() As Task(Of Boolean)
        Dim _bytes As Byte() = Nothing
        Try
            Debug.WriteLine($"BAND: setTimeFormatDisplay")

            If Await Connect() Then
                Dim doChar = Await GetCharacteristic(GetService(CustomBluetoothProfile.Basic.service), CustomBluetoothProfile.Basic.configurationCharacteristic)

                If Convert.ToBoolean(LocalSettings.Values("Is12hEnabled")) Then
                    _bytes = CustomBluetoothProfile.Basic.DATEFORMAT_TIME_12_HOURS
                Else
                    _bytes = CustomBluetoothProfile.Basic.DATEFORMAT_TIME_24_HOURS
                End If

                If Await doChar.WriteValueAsync(_bytes.AsBuffer) = GattCommunicationStatus.Success Then
                    Return True
                End If
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _bytes = Nothing
        End Try
    End Function

    Private Sub OnPropertyChanged(ByVal mPropertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(mPropertyName))
    End Sub

    Private Sub _HeartResult_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _HeartResult.PropertyChanged
        Try
            _DisplayItems.FirstOrDefault(Function(x) x.Operation = CustomMiBandResult.BandOperation.Heartrate).Value = DirectCast(sender, HeartResult).HeartRate.ToString("N0")

            _DisplayItems.FirstOrDefault(Function(x) x.Operation = CustomMiBandResult.BandOperation.Heartrate).OnPropertyChanged("HeartRate")
            _DisplayItems.FirstOrDefault(Function(x) x.Operation = CustomMiBandResult.BandOperation.Heartrate).OnPropertyChanged("Title")

        Catch ex As Exception
        End Try
    End Sub
End Class