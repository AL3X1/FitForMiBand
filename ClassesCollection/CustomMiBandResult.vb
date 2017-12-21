Imports Windows.ApplicationModel.Core

Public Class CustomMiBandResult
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Enum BandOperation
        Steps = 0
        Heartrate = 1
        Calories = 2
        Sleep = 3
        Distance = 4
        Battery = 5
        'Clock = 6
        Notifications = 7
        Welcome = 8
        ClockAndDate = 9
    End Enum

    Private _Operation As Integer
    Public Property Operation() As Integer
        Get
            Return _Operation
        End Get
        Set(ByVal value As Integer)
            _Operation = value
        End Set
    End Property

    Private _Value As String
    Public Property Value() As String
        Get
            Return _Value
        End Get
        Set(ByVal value As String)
            _Value = value
        End Set
    End Property

    Private _Unit As String
    Public Property Unit() As String
        Get
            Return _Unit
        End Get
        Set(ByVal value As String)
            _Unit = value
        End Set
    End Property

    Private _PictureUrl As String
    Public Property PictureUrl() As String
        Get
            Return _PictureUrl
        End Get
        Set(ByVal value As String)
            _PictureUrl = value
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

    Private _IsEnabled As Boolean
    Public Property IsEnabled() As Boolean
        Get
            Return _IsEnabled
        End Get
        Set(ByVal value As Boolean)
            _IsEnabled = value
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

    Private LocalSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

    Public Sub New()

    End Sub

    Public Sub New(ByVal mOperation As BandOperation, ByVal mValue As String, ByVal mTitle As String)
        _Operation = mOperation
        SetPictureOfOperation()
        SetUnitOfOperation()

        If LocalSettings.Values(String.Format("Setting_{0}", _Operation)) IsNot Nothing Then
            _IsEnabled = CBool(LocalSettings.Values(String.Format("Setting_{0}", _Operation)))
        Else
            _IsEnabled = False
        End If

        ' Override if Operation is Notification
        If _Operation = BandOperation.Notifications Then
            _IsEnabled = True
        End If

        _Value = mValue
        _Title = mTitle
    End Sub

    Public Sub New(ByVal mOperation As BandOperation)
        _Operation = mOperation
        SetPictureOfOperation()
        SetUnitOfOperation()

        If LocalSettings.Values(String.Format("Setting_{0}", _Operation)) IsNot Nothing Then
            _IsEnabled = CBool(LocalSettings.Values(String.Format("Setting_{0}", _Operation)))
        Else
            _IsEnabled = False
        End If

    End Sub

    Public Sub SetValue(ByVal mValue As String)
        _Value = mValue
        OnPropertyChanged("Value")
    End Sub

    Public Sub SetTitle(ByVal mTitle As String)
        _Title = mTitle
        OnPropertyChanged("Title")
    End Sub

    Private Sub SetPictureOfOperation()
        Dim mOperation As BandOperation = CType(_Operation, BandOperation)
        Try
            Select Case mOperation
                Case BandOperation.Battery
                    _PictureUrl = "ms-appx:///Assets/Symbols/akku.png"
                Case BandOperation.Calories
                    _PictureUrl = "ms-appx:///Assets/Symbols/cals.png"
                Case BandOperation.Distance
                    _PictureUrl = "ms-appx:///Assets/Symbols/distance.png"
                Case BandOperation.Heartrate
                    _PictureUrl = "ms-appx:///Assets/Symbols/rate.png"
                Case BandOperation.Sleep
                    _PictureUrl = "ms-appx:///Assets/Symbols/sleep.png"
                Case BandOperation.Steps
                    _PictureUrl = "ms-appx:///Assets/Symbols/steps.png"
                Case BandOperation.Notifications
                    _PictureUrl = "ms-appx:///Assets/Symbols/message.png"
                Case BandOperation.Welcome
                    _PictureUrl = "ms-appx:///Assets/Symbols/welcome.png"
            End Select
        Catch ex As Exception

        Finally
            mOperation = Nothing
        End Try
    End Sub

    Private Sub SetUnitOfOperation()
        Dim mOperation As BandOperation = CType(_Operation, BandOperation)
        Try
            Select Case mOperation
                Case BandOperation.Battery
                    _Unit = "%"
                Case BandOperation.Calories
                    _Unit = "cals"
                Case BandOperation.Distance
                    _Unit = "meter"
                Case BandOperation.Heartrate
                    _Unit = "bpm"
                Case BandOperation.Sleep
                    _Unit = "hours"
                Case BandOperation.Steps
                    _Unit = "steps"
                Case BandOperation.Notifications
                    _Unit = "active"
                Case Else
                    _Unit = ""
            End Select
        Catch ex As Exception

        Finally
            mOperation = Nothing
        End Try
    End Sub

    Public Async Sub OnPropertyChanged(ByVal mProperty As String)
        Await CoreApplication.Views.First().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, Sub()
                                                                                                                 RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(mProperty))
                                                                                                             End Sub)
    End Sub

End Class
