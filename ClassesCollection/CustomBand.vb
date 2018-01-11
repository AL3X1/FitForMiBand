Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Bluetooth.GenericAttributeProfile
Imports Windows.Devices.Enumeration

Public Class CustomBand

    Private _DeviceId As String
    Public Property DeviceId() As String
        Get
            Return _DeviceId
        End Get
        Set(ByVal value As String)
            _DeviceId = value
        End Set
    End Property

    Private _IsAuthNeeded As Boolean
    Public Property IsAuthNeeded() As Boolean
        Get
            Return _IsAuthNeeded
        End Get
        Set(ByVal value As Boolean)
            _IsAuthNeeded = value
        End Set
    End Property

    Private _Device As BluetoothLEDevice
    Private _GattServices As List(Of CustomGattService)

    Public Sub New()
        ' ToDo: Load from Settings
        _DeviceId = Nothing
        _IsAuthNeeded = True
        _GattServices = New List(Of CustomGattService)
    End Sub

    Public Async Function GetDeviceByNameAsync() As Task(Of Boolean)
        Dim _DeviceInformation As DeviceInformation = Nothing
        Dim _DeviceSelector As String = Nothing
        Try
            _DeviceId = Nothing
            _DeviceSelector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(True)

            For Each _DeviceInformation In Await DeviceInformation.FindAllAsync(_DeviceSelector)
                If _DeviceInformation.Name.ToUpper = "MI BAND 2" Then
                    _DeviceId = _DeviceInformation.Id
                    Return True
                End If
            Next

            Return False

        Catch ex As Exception
            Return False
        Finally
            _DeviceInformation = Nothing
            _DeviceSelector = Nothing
        End Try
    End Function

    Public Async Function Connect() As Task(Of Boolean)
        Dim _GattDeviceServicesResult As GattDeviceServicesResult = Nothing
        Dim _CustomGattService As CustomGattService = Nothing
        Try
            _Device = Await BluetoothLEDevice.FromIdAsync(_DeviceId)
            If _Device IsNot Nothing Then
                _GattDeviceServicesResult = Await _Device.GetGattServicesAsync(BluetoothCacheMode.Uncached)
                If _GattDeviceServicesResult.Status = GattCommunicationStatus.Success Then
                    For i = 0 To _GattDeviceServicesResult.Services.Count - 1
                        _CustomGattService = New CustomGattService(_GattDeviceServicesResult.Services(i))
                        If Await _CustomGattService.Initialize() Then
                            _GattServices.Add(_CustomGattService)
                        End If
                    Next
                Else
                    Return False
                End If
            Else
                Return False
            End If

            Return True

        Catch ex As Exception
            Return False
        Finally
            _CustomGattService = Nothing
            _GattDeviceServicesResult = Nothing
        End Try
    End Function

    Public Async Function AuthorizeOnDeviceAsync() As Task(Of Boolean)
        Try
            If Await Connect() Then

                Dim r = _GattServices.FirstOrDefault(Function(x) x.GattService.Uuid = CustomBluetoothProfile.Authentication.service).GattCharacteristics.FirstOrDefault(Function(x) x.GattCharacteristic.Uuid = CustomBluetoothProfile.Authentication.authCharacteristic)

                Dim erg = Await r.WriteValueAsync((New Byte() {100, 20}))

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

    Private Sub Disconnect()
        _GattServices = Nothing
        If _Device IsNot Nothing Then _Device.Dispose()
        _Device = Nothing
    End Sub
End Class

Public Class CustomGattService

    Private _GattService As GattDeviceService
    Public Property GattService() As GattDeviceService
        Get
            Return _GattService
        End Get
        Set(ByVal value As GattDeviceService)
            _GattService = value
        End Set
    End Property

    Private _GattCharacteristics As List(Of CustomGattCharacteristic)
    Public Property GattCharacteristics() As List(Of CustomGattCharacteristic)
        Get
            Return _GattCharacteristics
        End Get
        Set(ByVal value As List(Of CustomGattCharacteristic))
            _GattCharacteristics = value
        End Set
    End Property

    Public Sub New()
        _GattCharacteristics = New List(Of CustomGattCharacteristic)
    End Sub

    Public Sub New(ByVal mGattService As GattDeviceService)
        _GattService = mGattService
        _GattCharacteristics = New List(Of CustomGattCharacteristic)
    End Sub

    Public Async Function Initialize() As Task(Of Boolean)
        Dim _GattCharacteristicsResult As GattCharacteristicsResult
        Dim _GattCharacteristic As CustomGattCharacteristic = Nothing
        Try
            _GattCharacteristicsResult = Await _GattService.GetCharacteristicsAsync
            If _GattCharacteristicsResult.Status = GattCommunicationStatus.Success Then
                For i = 0 To _GattCharacteristicsResult.Characteristics.Count - 1
                    _GattCharacteristic = New CustomGattCharacteristic(_GattCharacteristicsResult.Characteristics(i))
                    Await _GattCharacteristic.Initialize()
                    _GattCharacteristics.Add(_GattCharacteristic)
                Next
            Else
                Return False
            End If

            Return True

        Catch ex As Exception
            Return False
        Finally
            _GattCharacteristicsResult = Nothing
            _GattCharacteristic = Nothing
        End Try
    End Function

End Class

Public Class CustomGattCharacteristic
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private WithEvents _GattCharacteristic As GattCharacteristic
    Public Property GattCharacteristic() As GattCharacteristic
        Get
            Return _GattCharacteristic
        End Get
        Set(ByVal value As GattCharacteristic)
            _GattCharacteristic = value
        End Set
    End Property

    Private _Properties As GattCharacteristicProperties
    Public Property Properties() As GattCharacteristicProperties
        Get
            Return _Properties
        End Get
        Set(ByVal value As GattCharacteristicProperties)
            _Properties = value
        End Set
    End Property

    Private _Value As Byte()
    Public Property Value() As Byte()
        Get
            Return _Value
        End Get
        Set(ByVal value As Byte())
            _Value = value
        End Set
    End Property

    Public Sub New()
    End Sub

    Public Sub New(ByVal mGattCharacteristic As GattCharacteristic)
        _GattCharacteristic = mGattCharacteristic
        _Properties = _GattCharacteristic.CharacteristicProperties
    End Sub

    Public Async Function Initialize() As Task(Of Boolean)
        Try
            Select Case _Properties
                Case GattCharacteristicProperties.Notify
                    Await _GattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify)
                Case Else

            End Select

            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Async Function WriteValueAsync(ByVal writeBytes As Byte()) As Task(Of Boolean)
        Dim _GattCommunicationStatus As GattCommunicationStatus = Nothing
        Try
            _GattCommunicationStatus = Await _GattCharacteristic.WriteValueAsync(writeBytes.AsBuffer, GattWriteOption.WriteWithoutResponse)
            If _GattCommunicationStatus = GattCommunicationStatus.Success Then
                Return True
            End If

            Return False

        Catch ex As Exception
            Debug.WriteLine($"WriteValue: {ex.Message}")
            Return False
        Finally
            _GattCommunicationStatus = Nothing
        End Try
    End Function

    Public Async Function ReadValueAsync() As Task(Of Byte())
        Dim _GattReadResult As GattReadResult = Nothing
        Try
            _GattReadResult = Await _GattCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached)
            If _GattReadResult.Status = GattCommunicationStatus.Success Then
                Return _GattReadResult.Value.ToArray
            End If

            Return Nothing

        Catch ex As Exception
            Debug.WriteLine($"ReadValue: {ex.Message}")
            Return Nothing
        Finally
            _GattReadResult = Nothing
        End Try
    End Function

    Private Sub _GattCharacteristic_ValueChanged(sender As GattCharacteristic, args As GattValueChangedEventArgs) Handles _GattCharacteristic.ValueChanged
        Try
            OnValueChanged("Value")

        Catch ex As Exception
            Debug.WriteLine($"ValueChanged: {ex.Message}")
        End Try
    End Sub

    Private Sub OnValueChanged(ByVal mProperty As String)
        Try
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(mProperty))

        Catch ex As Exception
            Debug.WriteLine($"OnValueChanged: {ex.Message}")
        End Try
    End Sub

End Class