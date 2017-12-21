Public Class ClockResult

    Private _DisplayValue As DateTime
    Public Property DispayValue() As DateTime
        Get
            Return _DisplayValue
        End Get
        Set(ByVal value As DateTime)
            _DisplayValue = value
        End Set
    End Property

    Public Sub New(ByVal mValue As DateTime)
        _DisplayValue = mValue
    End Sub

End Class
