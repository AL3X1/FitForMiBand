Public Class CustomComboBoxItem

    Private _Text As String
    Public Property Text() As String
        Get
            Return _Text
        End Get
        Set(ByVal value As String)
            _Text = value
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

    Public Sub New()

    End Sub

    Public Sub New(mText As String, mValue As String)
        _Text = mText
        _Value = mValue
    End Sub

End Class
