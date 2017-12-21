Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Bluetooth.GenericAttributeProfile

Public Class StepResult
    Implements IDisposable

    Private _TotalSteps As Integer
    Public Property TotalSteps() As Integer
        Get
            Return _TotalSteps
        End Get
        Set(ByVal value As Integer)
            _TotalSteps = value
        End Set
    End Property

    Private _History As List(Of HistoryValues)
    Public Property History() As List(Of HistoryValues)
        Get
            Return _History
        End Get
        Set(ByVal value As List(Of HistoryValues))
            _History = value
        End Set
    End Property

    Private _StepsReachedInPercent As String
    Public Property StepsReachedInPercent() As String
        Get
            Return _StepsReachedInPercent
        End Get
        Set(ByVal value As String)
            _StepsReachedInPercent = value
        End Set
    End Property

    Private _TotalDistance As Double
    Public Property TotalDistance() As Double
        Get
            Return _TotalDistance
        End Get
        Set(ByVal value As Double)
            _TotalDistance = value
        End Set
    End Property

    Private _TotalCals As Double
    Public Property TotalCals() As Double
        Get
            Return _TotalCals
        End Get
        Set(ByVal value As Double)
            _TotalCals = value
        End Set
    End Property

    Public Shared ReadOnly REALTIME_STEPS As Guid = New Guid("00000007-0000-3512-2118-0009af100700".ToUpper)
    Public Shared ReadOnly SERVICE As Guid = New Guid("0000fee0-0000-1000-8000-00805f9b34fb".ToUpper)

    Private LocalSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

    Public Sub New()

    End Sub

    Public Function Initialize() As Boolean
        Dim _data As StepResult = Nothing
        Try
            If LocalSettings.Values("StepResult") IsNot Nothing Then
                _data = DirectCast(Helpers.FromXml(LocalSettings.Values("StepResult").ToString, GetType(StepResult)), StepResult)

                _TotalSteps = _data.TotalSteps
                _TotalDistance = _data.TotalDistance
                _TotalCals = _data.TotalCals
                _History = _data.History
                _StepsReachedInPercent = _data.StepsReachedInPercent

                Return True
            Else
                _History = New List(Of HistoryValues)
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            _data = Nothing
        End Try
    End Function

    Private Sub AddToHistory(ByVal type As HistoryValues.Types, ByVal value As Double)
        Dim History As HistoryValues = Nothing
        Try
            History = _History.FirstOrDefault(Function(x) x.Type = type And x.Moment.Date = DateTime.Now.Date)
            If History IsNot Nothing Then
                History.Value = value
            Else
                _History.Add(New HistoryValues(type, DateTime.Now, value))
            End If
        Catch ex As Exception
        Finally
            History = Nothing
        End Try
    End Sub

    Public Async Function GetSteps(ByVal mCharacteristic As GattCharacteristic) As Task(Of GattCommunicationStatus)
        Try
            Dim Result = Await mCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached)
            If Result.Status = GattCommunicationStatus.Success Then
                Debug.WriteLine("READ: STEPS, DISTANCE, CALORIES")

                Dim ResultBytes = Result.Value.ToArray

                '  Daily total steps
                _TotalSteps = ((ResultBytes(1) And 255) Or ((ResultBytes(2) And 255) << 8))
                AddToHistory(HistoryValues.Types.Steps, _TotalSteps)

                _StepsReachedInPercent = String.Format("{0}% Complete", (_TotalSteps * 100 / CDbl(LocalSettings.Values("Profile_Steps"))).ToString("N0"))

                ' Daily total distance
                _TotalDistance = ((ResultBytes(5) And 255) Or (ResultBytes(6) And 255) << 8 Or (ResultBytes(7) And 255) Or (ResultBytes(8) And 255) << 24)
                AddToHistory(HistoryValues.Types.Distances, _TotalDistance)

                ' Get calories
                _TotalCals = ((ResultBytes(9) And 255) Or ((ResultBytes(10) And 255) << 8) Or (ResultBytes(11) And 255) + (ResultBytes(12) And 255) << 24)
                AddToHistory(HistoryValues.Types.Calories, _TotalCals)


                'let steps = (UInt16(buffer[1] & 255) | (UInt16(buffer[2] & 255) << 8))
                'let distance = (((UInt32(buffer[5] & 255) | (UInt32(buffer[6] & 255) << 8)) | UInt32(buffer[7] & 255)) | (UInt32(buffer[8] & 255) << 24));
                'let calories = (((UInt32(buffer[9] & 255) | (UInt32(buffer[10] & 255) << 8)) | UInt32(buffer[11] & 255)) | (UInt32(buffer[12] & 255) << 24));

                LocalSettings.Values("StepResult") = Helpers.ToXml(Me, GetType(StepResult))

                Return Result.Status
            Else
                Return Result.Status
            End If

        Catch ex As Exception
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

Public Class HistoryValues

    Public Enum Types
        Steps = 0
        Distances = 1
        Calories = 2
    End Enum

    Private _Type As Integer
    Public Property Type() As Integer
        Get
            Return _Type
        End Get
        Set(ByVal value As Integer)
            _Type = value
        End Set
    End Property

    Private _Moment As Date
    Public Property Moment() As Date
        Get
            Return _Moment
        End Get
        Set(ByVal value As Date)
            _Moment = value
        End Set
    End Property

    Private _Value As Double
    Public Property Value() As Double
        Get
            Return _Value
        End Get
        Set(ByVal value As Double)
            _Value = value
        End Set
    End Property

    Public Sub New()
    End Sub

    Public Sub New(ByVal mType As Types, ByVal mMoment As Date, ByVal mValue As Double)
        _Type = mType
        _Moment = mMoment
        _Value = mValue
    End Sub

End Class
