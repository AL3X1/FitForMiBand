Imports System.Xml.Serialization
Imports Windows.Storage.Streams
Imports Windows.UI.Notifications

Public Class NotificationRequest

    Public Enum DisplayMethods
        AppIconOnly = 0
        MessageTitle = 1
        AppDisplayName = 2
    End Enum

    Private _DisplayName As String
    Public Property DisplayName() As String
        Get
            Return _DisplayName
        End Get
        Set(ByVal value As String)
            _DisplayName = value
        End Set
    End Property

    Private _Id As String
    Public Property Id() As String
        Get
            Return _Id
        End Get
        Set(ByVal value As String)
            _Id = value
        End Set
    End Property

    Private _IsOn As Boolean
    Public Property IsOn() As Boolean
        Get
            Return _IsOn
        End Get
        Set(ByVal value As Boolean)
            _IsOn = value
        End Set
    End Property

    Private _DisplayMethod As Integer
    Public Property DisplayMethod() As Integer
        Get
            Return _DisplayMethod
        End Get
        Set(ByVal value As Integer)
            _DisplayMethod = value
        End Set
    End Property

    Private _DisplayMethodAsText As String
    Public Property DisplayMethodAsText() As String
        Get
            Return _DisplayMethodAsText
        End Get
        Set(ByVal value As String)
            _DisplayMethodAsText = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(AppInfo As AppInfo)
        _DisplayName = AppInfo.DisplayInfo.DisplayName
        _Id = AppInfo.AppUserModelId
        _IsOn = True
        _DisplayMethod = DisplayMethods.AppDisplayName

        getMethodText()
    End Sub

    Private Sub getMethodText()
        Select Case _DisplayMethod
            Case DisplayMethods.AppDisplayName
                _DisplayMethodAsText = "Display App-Name"
            Case DisplayMethods.AppIconOnly
                _DisplayMethodAsText = "Display App-Icon"
            Case DisplayMethods.MessageTitle
                _DisplayMethodAsText = "Display Message"
        End Select
    End Sub

End Class
