Imports ClassesCollection

Public NotInheritable Class StepsPage
    Inherits Page

    Private _Result As StepResult
    Public Property Result() As StepResult
        Get
            Return _Result
        End Get
        Set(ByVal value As StepResult)
            _Result = value
        End Set
    End Property

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        _Result = TryCast(e.Parameter, StepResult)
    End Sub

    Private Sub StepsPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Me.lblTitle.Text = _Result.StepsReachedInPercent
            Me.lblSteps.Text = _Result.TotalSteps.ToString("N0")
            Me.lblDistanceDetailsDay.Text = GetDistanceDayInKm()
            Me.lblCalsDetailsDay.Text = _Result.TotalCals.ToString("N0")
            Me.rpbcDailySteps.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
            Me.rpbcDailySteps.Value = _Result.TotalSteps

            Me.lblDistanceDetailsWeek.Text = GetDistanceWeekInKm()
            Me.lblCalsDetailsWeek.Text = GetCalsWeek()

            Dim i As Integer = 1

            For Each mElement In _Result.History.Where(Function(x) x.Type = 0 And x.Moment <= DateTime.Now And x.Moment >= DateTime.Now.AddDays(-7)).ToList
                Select Case i
                    Case 1
                        lblDay1.Text = mElement.Moment.Date.ToString("ddd")
                        pbDay1.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
                        pbDay1.Value = mElement.Value
                        pbDay1.Minimum = 0
                        lblValueDay1.Text = mElement.Value.ToString("N0")
                    Case 2
                        lblDay2.Text = mElement.Moment.Date.ToString("ddd")
                        pbDay2.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
                        pbDay2.Value = mElement.Value
                        pbDay2.Minimum = 0
                        lblValueDay2.Text = mElement.Value.ToString("N0")
                    Case 3
                        lblDay3.Text = mElement.Moment.Date.ToString("ddd")
                        pbDay3.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
                        pbDay3.Value = mElement.Value
                        pbDay3.Minimum = 0
                        lblValueDay3.Text = mElement.Value.ToString("N0")
                    Case 4
                        lblDay4.Text = mElement.Moment.Date.ToString("ddd")
                        pbDay4.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
                        pbDay4.Value = mElement.Value
                        pbDay4.Minimum = 0
                        lblValueDay4.Text = mElement.Value.ToString("N0")
                    Case 5
                        lblDay5.Text = mElement.Moment.Date.ToString("ddd")
                        pbDay5.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
                        pbDay5.Value = mElement.Value
                        pbDay5.Minimum = 0
                        lblValueDay5.Text = mElement.Value.ToString("N0")
                    Case 6
                        lblDay6.Text = mElement.Moment.Date.ToString("ddd")
                        pbDay6.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
                        pbDay6.Value = mElement.Value
                        pbDay6.Minimum = 0
                        lblValueDay6.Text = mElement.Value.ToString("N0")
                    Case 7
                        lblDay7.Text = mElement.Moment.Date.ToString("ddd")
                        pbDay7.Maximum = Convert.ToInt32(App.LocalSettings.Values("Profile_Steps"))
                        pbDay7.Value = mElement.Value
                        pbDay7.Minimum = 0
                        lblValueDay7.Text = mElement.Value.ToString("N0")
                End Select



                i += 1
            Next

        Catch ex As Exception

        End Try
    End Sub

    Private Function GetDistanceWeekInKm() As String
        Try
            Return (_Result.History.Where(Function(x) x.Type = HistoryValues.Types.Distances And x.Moment <= DateTime.Now And x.Moment >= DateTime.Now.AddDays(-7)).Sum(Function(x) x.Value) / 1000).ToString("N2")
        Catch ex As Exception
            Return (0.0).ToString("N2")
        End Try
    End Function

    Private Function GetDistanceDayInKm() As String
        Try
            Return (_Result.TotalDistance / 1000).ToString("N2")
        Catch ex As Exception
            Return (0).ToString("N2")
        End Try
    End Function

    Private Function GetCalsWeek() As String
        Try
            Return (_Result.History.Where(Function(x) x.Type = HistoryValues.Types.Calories And x.Moment <= DateTime.Now And x.Moment >= DateTime.Now.AddDays(-7)).Sum(Function(x) x.Value) / 1000).ToString("N2")
        Catch ex As Exception
            Return (0).ToString("N2")
        End Try
    End Function
End Class
