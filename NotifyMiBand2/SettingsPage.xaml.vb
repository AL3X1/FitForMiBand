' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
Imports Windows.ApplicationModel.Background
Imports Windows.Foundation.Metadata
Imports Windows.UI.Notifications.Management
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class SettingsPage
    Inherits Page

    Private bolLoading As Boolean = True

    Public Sub New()

        InitializeComponent()

        Dim DisplayItemsString As New List(Of String)
        DisplayItemsString.Add("Clock")

        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Steps))) IsNot Nothing Then
            If Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Steps)))) = True Then
                DisplayItemsString.Add("Steps")
            End If
        End If

        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Distance))) IsNot Nothing Then
            If Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Distance)))) = True Then
                DisplayItemsString.Add("Distance")
            End If
        End If

        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Calories))) IsNot Nothing Then
            If Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Calories)))) = True Then
                DisplayItemsString.Add("Calories")
            End If
        End If

        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Heartrate))) IsNot Nothing Then
            If Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Heartrate)))) = True Then
                DisplayItemsString.Add("Heart-Rate")
            End If
        End If

        If App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Battery))) IsNot Nothing Then
            If Convert.ToBoolean(App.LocalSettings.Values(String.Format("Setting_{0}", CInt(CustomMiBandResult.BandOperation.Battery)))) = True Then
                DisplayItemsString.Add("Battery")
            End If
        End If

        Me.lblDisplayItems.Text = String.Join(", ", DisplayItemsString)

        ' Check if Windows Version supports UserNotificationListener
        If (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener")) Then
            App.LocalSettings.Values(String.Format("Setting_{0}_IsEnabled", CInt(CustomMiBandResult.BandOperation.Notifications))) = True
        Else
            App.LocalSettings.Values(String.Format("Setting_{0}_IsEnabled", CInt(CustomMiBandResult.BandOperation.Notifications))) = False
        End If

        If App.LocalSettings.Values("IsDisplayOnLiftWristEnabled") IsNot Nothing Then tsDisplay.IsOn = Convert.ToBoolean(App.LocalSettings.Values("IsDisplayOnLiftWristEnabled"))
        If App.LocalSettings.Values("Is12hEnabled") IsNot Nothing Then tsTimeformat.IsOn = Convert.ToBoolean(App.LocalSettings.Values("Is12hEnabled"))
        If App.LocalSettings.Values("IsDateEnabled") IsNot Nothing Then tsDate.IsOn = Convert.ToBoolean(App.LocalSettings.Values("IsDateEnabled"))
        If App.LocalSettings.Values("IsGoalNotificationEnabled") IsNot Nothing Then tsGoal.IsOn = Convert.ToBoolean(App.LocalSettings.Values("IsGoalNotificationEnabled"))
        If App.LocalSettings.Values("IsRotateWristToSwitchInfoEnabled") IsNot Nothing Then tsRotate.IsOn = Convert.ToBoolean(App.LocalSettings.Values("IsRotateWristToSwitchInfoEnabled"))
        If App.LocalSettings.Values("IsWearLocationRightEnabled") IsNot Nothing Then
            If Convert.ToBoolean(App.LocalSettings.Values("IsWearLocationRightEnabled")) Then
                rbRightHand.IsChecked = True
            Else
                rbLeftHand.IsChecked = True
            End If
        Else
            rbRightHand.IsChecked = True
        End If
        If App.LocalSettings.Values("IsDndEnabled") IsNot Nothing Then tsDnD.IsOn = Convert.ToBoolean(App.LocalSettings.Values("IsDndEnabled"))

        If App.LocalSettings.Values("PeriodicSync") IsNot Nothing Then
            chkPeriodicSync.SelectedValue = App.LocalSettings.Values("PeriodicSync")
        End If

        bolLoading = False

    End Sub

    Private Async Sub tsDisplay_Toggled(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, ToggleSwitch).IsEnabled = False
            Try
                App.LocalSettings.Values("IsDisplayOnLiftWristEnabled") = DirectCast(sender, ToggleSwitch).IsOn
                Await App.CustomMiBand.setActivateDisplayOnLiftWrist()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, ToggleSwitch).IsEnabled = True
            End Try
        End If
    End Sub

    Private Async Sub tsTimeformat_Toggled(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, ToggleSwitch).IsEnabled = False
            Try
                App.LocalSettings.Values("Is12hEnabled") = DirectCast(sender, ToggleSwitch).IsOn
                Await App.CustomMiBand.setTimeFormatDisplay()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, ToggleSwitch).IsEnabled = True
            End Try
        End If
    End Sub

    Private Async Sub tsDate_Toggled(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, ToggleSwitch).IsEnabled = False
            Try
                App.LocalSettings.Values("IsDateEnabled") = DirectCast(sender, ToggleSwitch).IsOn
                Await App.CustomMiBand.setDateDisplay()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, ToggleSwitch).IsEnabled = True
            End Try
        End If
    End Sub

    Private Sub tsGoal_Toggled(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, ToggleSwitch).IsEnabled = False
            Try
                App.LocalSettings.Values("IsGoalNotificationEnabled") = DirectCast(sender, ToggleSwitch).IsOn
                App.CustomMiBand.setGoalNotification()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, ToggleSwitch).IsEnabled = True
            End Try
        End If
    End Sub

    Private Sub hlbMenu_Click(sender As Object, e As RoutedEventArgs)
        Frame.Navigate(GetType(DisplayitemsPage))
    End Sub

    Private Sub chkPeriodicSync_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If chkPeriodicSync.SelectedIndex > -1 Then
            App.LocalSettings.Values("PeriodicSync") = chkPeriodicSync.SelectedItem.ToString
        Else
            App.LocalSettings.Values("PeriodicSync") = "Off"
        End If

        ' Kill Task
        Dim Task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(Function(x) x.Name = "PeriodicalSync")
        If Task IsNot Nothing Then
            Task.Unregister(True)
        End If

    End Sub

    Private Sub tsDnD_Toggled(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, ToggleSwitch).IsEnabled = False
            Try
                App.LocalSettings.Values("IsDndEnabled") = DirectCast(sender, ToggleSwitch).IsOn
                App.CustomMiBand.setDoNotDisturb()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, ToggleSwitch).IsEnabled = True
            End Try
        End If
    End Sub

    Private Sub tsRotate_Toggled(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, ToggleSwitch).IsEnabled = False
            Try
                App.LocalSettings.Values("IsRotateWristToSwitchInfoEnabled") = DirectCast(sender, ToggleSwitch).IsOn
                App.CustomMiBand.setRotateWristToSwitchInfo()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, ToggleSwitch).IsEnabled = True
            End Try
        End If
    End Sub

    Private Async Sub rbRightHand_Click(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, RadioButton).IsEnabled = False
            Try
                App.LocalSettings.Values("IsWearLocationRightEnabled") = rbRightHand.IsChecked
                Await App.CustomMiBand.setWearLocation()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, RadioButton).IsEnabled = True
            End Try
        End If
    End Sub

    Private Async Sub rbLeftHand_Click(sender As Object, e As RoutedEventArgs)
        If bolLoading = False Then
            pbProcessing.Visibility = Visibility.Visible
            DirectCast(sender, RadioButton).IsEnabled = False
            Try
                App.LocalSettings.Values("IsWearLocationRightEnabled") = rbRightHand.IsChecked
                Await App.CustomMiBand.setWearLocation()

            Catch ex As Exception
                Helpers.DebugWriter(GetType(SettingsPage), ex.Message)
            Finally
                pbProcessing.Visibility = Visibility.Collapsed
                DirectCast(sender, RadioButton).IsEnabled = True
            End Try
        End If
    End Sub
End Class
