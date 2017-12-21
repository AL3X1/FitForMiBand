Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Bluetooth.GenericAttributeProfile

Public Class BatteryResult
    Implements IDisposable

    Private _Percentage As Double
    Public Property Percentage() As Double
        Get
            Return _Percentage
        End Get
        Set(ByVal value As Double)
            _Percentage = value
        End Set
    End Property

    Private _Charging As Boolean
    Public Property Charging() As Boolean
        Get
            Return _Charging
        End Get
        Set(ByVal value As Boolean)
            _Charging = value
        End Set
    End Property

    Private _Cycles As Integer
    Public Property Cycles() As Integer
        Get
            Return _Cycles
        End Get
        Set(ByVal value As Integer)
            _Cycles = value
        End Set
    End Property

    Private _LastChargingDate As DateTime
    Public Property LastChargingDate() As DateTime
        Get
            Return _LastChargingDate
        End Get
        Set(ByVal value As DateTime)
            _LastChargingDate = value
        End Set
    End Property

    Public Shared ReadOnly SERVICE As Guid = New Guid("0000fee0-0000-1000-8000-00805f9b34fb".ToUpper)
    Public Shared ReadOnly BATTERY_INFO As Guid = New Guid("00000006-0000-3512-2118-0009af100700".ToUpper)

    Private Const DEVICE_BATTERY_NORMAL As Integer = 0
    Private Const DEVICE_BATTERY_CHARGING As Integer = 1

    Private LocalSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

    Public Sub New()
    End Sub

    Public Function Initialize() As Boolean
        Dim _data As BatteryResult = Nothing
        Try
            If LocalSettings.Values("BatteryResult") IsNot Nothing Then
                _data = DirectCast(Helpers.FromXml(LocalSettings.Values("BatteryResult").ToString, GetType(BatteryResult)), BatteryResult)

                _Percentage = _data.Percentage
                _Charging = _data.Charging
                _Cycles = _data.Cycles
                _LastChargingDate = _data.LastChargingDate

                Return True
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _data = Nothing
        End Try
    End Function

    Public Async Function GetBatteryInfo(ByVal mCharacteristic As GattCharacteristic) As Task(Of GattCommunicationStatus)
        Try
            Dim BatteryResult = Await mCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached)
            If BatteryResult.Status = GattCommunicationStatus.Success Then
                Debug.WriteLine("READ: BATTERY INFO")

                Dim BatteryResultBytes = BatteryResult.Value.ToArray

                ' resultBytes(1) = % charged
                ' resultBytes(2) = state: normal or charging
                ' mData[12] = last charged year + 201
                ' mData[13] = last charged month
                ' mData[14] = last charged day
                ' mData[15] = last charged hour
                ' mData[16] = last charged minute
                ' mData[17] = last charged second
                ' mData[18] = cycles

                If BatteryResultBytes(2) = DEVICE_BATTERY_CHARGING Then
                    _Charging = True
                Else
                    _Charging = False
                End If

                _Percentage = BatteryResultBytes(1)
                _Cycles = BatteryResultBytes(18)
                _LastChargingDate = CDate(String.Format("{0}.{1}.201{2} {3}:{4}:{5}", BatteryResultBytes(13), BatteryResultBytes(14), BatteryResultBytes(12), BatteryResultBytes(15), BatteryResultBytes(16), BatteryResultBytes(17)))


                LocalSettings.Values("BatteryResult") = Helpers.ToXml(Me, GetType(BatteryResult))

                Return BatteryResult.Status
            Else
                Return BatteryResult.Status
            End If

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
            Return GattCommunicationStatus.ProtocolError
        End Try
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' Dient zur Erkennung redundanter Aufrufe.

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
