' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class BatteryPage
    Inherits Page

    Private _Result As BatteryResult
    Public Property Result() As BatteryResult
        Get
            Return _Result
        End Get
        Set(ByVal value As BatteryResult)
            _Result = value
        End Set
    End Property

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        _Result = TryCast(e.Parameter, BatteryResult)
    End Sub

    Private Sub BatteryPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If _Result IsNot Nothing Then
            Me.lblPercentage.Text = String.Format("{0} %", _Result.Percentage)
            Me.lblBefore.Text = ""
            Me.lblChargingDate.Text = String.Format("Last charging date: {0}", _Result.LastChargingDate.ToString("dd.MM.yyyy HH:mm"))
            Me.lblEstimated.Text = GetEstimatedTime()
            Me.pgPercentage.Value = _Result.Percentage

        End If
    End Sub

    Private Function GetEstimatedTime() As String
        ' 20 Tage laut Hersteller (Akkulaufzeit) = 28800 Minuten
        Dim EstimatedMinutes = 28800 / 100 * _Result.Percentage
        Dim EstimatedMinutesCalculated As TimeSpan = DateTime.Now.AddMinutes(EstimatedMinutes) - DateTime.Now

        Return String.Format("Estimated time remaining: {0} days {1} hours {2} minutes", EstimatedMinutesCalculated.Days, EstimatedMinutesCalculated.Hours, EstimatedMinutesCalculated.Minutes)

    End Function

    Private Sub sliderPowerSaving_ValueChanged(sender As Object, e As RangeBaseValueChangedEventArgs)
        If Me.lblDisplaySliderValue IsNot Nothing Then Me.lblDisplaySliderValue.Text = String.Format("{0}%", Me.sliderPowerSaving.Value.ToString)
    End Sub

    Private Sub chkSlider_Checked(sender As Object, e As RoutedEventArgs)
        Me.sliderPowerSaving.IsEnabled = Convert.ToBoolean(chkSlider.IsChecked)
    End Sub

    Private Sub chkSlider_Unchecked(sender As Object, e As RoutedEventArgs)
        Me.sliderPowerSaving.IsEnabled = Convert.ToBoolean(chkSlider.IsChecked)
    End Sub
End Class
