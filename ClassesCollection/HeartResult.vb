Imports Windows.ApplicationModel.Core
Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Bluetooth.GenericAttributeProfile

Public Class HeartResult
    Implements IDisposable
    Implements INotifyPropertyChanged

    Private _HeartRate As Double
    Public Property HeartRate() As Double
        Get
            Return _HeartRate
        End Get
        Set(ByVal value As Double)
            _HeartRate = value
        End Set
    End Property

    Private _LastCheckDate As DateTime
    Public Property LastCheckDate() As DateTime
        Get
            Return _LastCheckDate
        End Get
        Set(ByVal value As DateTime)
            _LastCheckDate = value
        End Set
    End Property

    Private _Title As String
    Public Property Title() As String
        Get
            Return _Title
        End Get
        Set(ByVal value As String)
            _Title = value
        End Set
    End Property

    Public Shared ReadOnly SERVICE As Guid = New Guid("0000180D-0000-1000-8000-00805F9B34FB".ToUpper)
    Public Shared ReadOnly HEARTRATE_MEASUREMENT As Guid = New Guid("00002A37-0000-1000-8000-00805F9B34FB".ToUpper)
    Public Shared ReadOnly HEARTRATE_CONTROL_POINT As Guid = New Guid("00002A39-0000-1000-8000-00805F9B34FB".ToUpper)

    Private ReadOnly COMMAND_START_HEART_RATE_MEASUREMENT As Byte() = New Byte() {21, 2, 1}
    Private ReadOnly COMMAND_SET_PERIODIC_HR_MEASUREMENT_INTERVAL As Byte = 14 '0x14


    Private LocalSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

    Public Sub New()
    End Sub

    Public Function Initialize() As Boolean
        Dim _data As HeartResult = Nothing
        Try
            If LocalSettings.Values("HeartResult") IsNot Nothing Then
                _data = DirectCast(Helpers.FromXml(LocalSettings.Values("HeartResult").ToString, GetType(HeartResult)), HeartResult)

                _HeartRate = _data.HeartRate
                _LastCheckDate = _data.LastCheckDate
                _Title = _data.Title

                Return True
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _data = Nothing
        End Try
    End Function

    Private Function getHeartRateMeasurementInterval() As Byte
        '  Return GBApplication.getPrefs().getInt("heartrate_measurement_interval", 0) / 60;
        Return 15
    End Function

    Public Async Function setHeartrateMeasurementInterval(ByVal mCharacteristic As GattCharacteristic) As Task(Of GattCommunicationStatus)
        Dim result As GattWriteResult = Nothing
        Try
            result = Await mCharacteristic.WriteValueWithResultAsync(New Byte() {COMMAND_SET_PERIODIC_HR_MEASUREMENT_INTERVAL, getHeartRateMeasurementInterval()}.AsBuffer)

            Return result.Status

        Catch ex As Exception
            Return GattCommunicationStatus.ProtocolError
        Finally
            result = Nothing
        End Try
    End Function

    Public Async Function GetHeartRateMeasurement(ByVal mNotifyCharacteristic As GattCharacteristic, ByVal mCharacteristic As GattCharacteristic) As Task(Of GattCommunicationStatus)
        Try
            If Await mNotifyCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) = GattCommunicationStatus.Success Then
                AddHandler mNotifyCharacteristic.ValueChanged, AddressOf ValueChanged
                Return Await mCharacteristic.WriteValueAsync(COMMAND_START_HEART_RATE_MEASUREMENT.AsBuffer)
            End If

            Return GattCommunicationStatus.ProtocolError

        Catch ex As Exception
            Return GattCommunicationStatus.ProtocolError
        End Try
    End Function

    Private Sub ValueChanged(sender As GattCharacteristic, args As GattValueChangedEventArgs)
        If sender.Uuid = HEARTRATE_MEASUREMENT Then
            GetHeartRate(args.CharacteristicValue.ToArray)
        End If
    End Sub

    Private Async Sub GetHeartRate(ByVal HeartResult As Byte())
        Try
            If HeartResult.Count >= 2 Then
                _HeartRate = HeartResult(1)
            End If

            _LastCheckDate = DateTime.Now
            _Title = String.Format("Last checked at: {0} h", _LastCheckDate.ToString("dddd, dd.MM.yyyy HH:mm"))

            LocalSettings.Values("HeartResult") = Helpers.ToXml(Me, GetType(HeartResult))

            Await CoreApplication.Views.First().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, Sub()
                                                                                                                     RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HeartRate"))
                                                                                                                 End Sub)

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
        End Try
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' Dient zur Erkennung redundanter Aufrufe.
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then

            End If
        End If
        disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region


End Class
