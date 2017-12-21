' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class HeartratePage
    Inherits Page

    Private WithEvents _Result As HeartResult
    Public Property Result() As HeartResult
        Get
            Return _Result
        End Get
        Set(ByVal value As HeartResult)
            _Result = value
        End Set
    End Property

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        _Result = TryCast(e.Parameter, HeartResult)
    End Sub

    Private Sub _Result_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _Result.PropertyChanged
        Try
            Debug.WriteLine($"Received: Heart-Rate in bpm")

            Me.lblStepsDetails.Text = _Result.HeartRate.ToString("N0")
            Me.lblBefore.Text = Helpers.TimeSpanToText(_Result.LastCheckDate)
        Catch ex As Exception
        Finally
            prMeasurement.IsActive = False
            btnMeasurement.IsEnabled = True
        End Try
    End Sub

    Private Sub HeartratePage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Me.lblBefore.Text = Helpers.TimeSpanToText(_Result.LastCheckDate)
        Me.lblStepsDetails.Text = _Result.HeartRate.ToString("N0")
    End Sub

    Private Async Sub btnMeasurement_Click(sender As Object, e As RoutedEventArgs)
        prMeasurement.IsActive = True
        btnMeasurement.IsEnabled = False
        Try
            Debug.WriteLine($"Request: Heart-Rate in bpm")

            Await App.CustomMiBand.SetAlertLevel(3)
            Await _Result.GetHeartRateMeasurement(Await App.CustomMiBand.GetCharacteristic(App.CustomMiBand.GetService(HeartResult.SERVICE), HeartResult.HEARTRATE_MEASUREMENT), Await App.CustomMiBand.GetCharacteristic(App.CustomMiBand.GetService(HeartResult.SERVICE), HeartResult.HEARTRATE_CONTROL_POINT))

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
            btnMeasurement.IsEnabled = True
            prMeasurement.IsActive = False
        Finally

        End Try
    End Sub
End Class
