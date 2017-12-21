Imports ClassesCollection
Imports Windows.ApplicationModel.Background
Imports Windows.UI.Notifications.Management

Public NotInheritable Class MainPage
    Inherits Page

    Private Async Sub btnSync_Click(sender As Object, e As RoutedEventArgs)
        pgWorking.Visibility = Visibility.Visible
        btnSync.IsEnabled = False
        Try

            If App.LocalSettings.Values("DeviceId") IsNot Nothing Then
                Await App.CustomMiBand.UpdateOperations()
            End If

            CustomMasterDetailsView.ItemsSource = Nothing
            CustomMasterDetailsView.ItemsSource = App.CustomMiBand.DisplayItems.Where(Function(x) x.IsEnabled = True).OrderBy(Function(x) x.Operation)

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
            pgWorking.Visibility = Visibility.Collapsed
            btnSync.IsEnabled = True
        Finally
            pgWorking.Visibility = Visibility.Collapsed
            btnSync.IsEnabled = True
        End Try
    End Sub

    Private Sub btnDevice_Click(sender As Object, e As RoutedEventArgs)
        Frame.Navigate(GetType(DevicePage))
    End Sub

    Private Async Function RegisterTaskForNotifications() As Task(Of Boolean)
        Dim Status As BackgroundAccessStatus = Nothing
        Dim Builder As BackgroundTaskBuilder = Nothing
        Try
            If TryCast(BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(Function(x) x.Name = "UserNotificationChanged"), BackgroundTaskRegistration) Is Nothing Then

                ' Check access status
                Status = Await BackgroundExecutionManager.RequestAccessAsync()
                If Not Status = BackgroundAccessStatus.AlwaysAllowed And Not Status = BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then
                    Return False
                End If

                ' Build the background task (using SPM (New Single-Process-Model => Annyversary Update)
                Builder = New BackgroundTaskBuilder With {.Name = "UserNotificationChanged", .TaskEntryPoint = GetType(BackgroundTaskRuntime.NotificationChanged).FullName}

                ' Set trigger for task
                Builder.SetTrigger(New UserNotificationChangedTrigger(Windows.UI.Notifications.NotificationKinds.Toast))

                ' register task
                Builder.Register()

                Return True
            Else
                Return True
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            Status = Nothing
            Builder = Nothing
        End Try
    End Function

    Private Async Function RegisterTaskForSync() As Task(Of Boolean)
        Dim Status As BackgroundAccessStatus = Nothing
        Dim Builder As BackgroundTaskBuilder = Nothing
        Dim PeriodTime As UInt32 = 15
        Try
            If App.LocalSettings.Values("PeriodicSync") IsNot Nothing Then
                If App.LocalSettings.Values("PeriodicSync").ToString = "Off" Then
                    Dim Task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(Function(x) x.Name = "PeriodicalSync")
                    If Task IsNot Nothing Then
                        Task.Unregister(True)
                        Return False
                    End If
                Else
                    PeriodTime = Convert.ToUInt32(App.LocalSettings.Values("PeriodicSync"))
                End If
            End If

            If TryCast(BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(Function(x) x.Name = "PeriodicalSync"), BackgroundTaskRegistration) Is Nothing Then

                ' Check access status
                Status = Await BackgroundExecutionManager.RequestAccessAsync()
                If Not Status = BackgroundAccessStatus.AlwaysAllowed And Not Status = BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then
                    Return False
                End If

                ' Build the background task (using SPM (New Single-Process-Model => Annyversary Update)
                Builder = New BackgroundTaskBuilder With {.Name = "PeriodicalSync", .TaskEntryPoint = GetType(BackgroundTaskRuntime.PeriodicalSync).FullName}

                ' Set trigger for task
                Builder.SetTrigger(New TimeTrigger(PeriodTime, False))

                ' register task
                Builder.Register()

                Return True
            Else
                Return True
            End If

            Return False

        Catch ex As Exception
            Return False
        Finally
            Status = Nothing
            Builder = Nothing
        End Try
    End Function

    Private Async Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            CustomMasterDetailsView.ItemsSource = Nothing
            CustomMasterDetailsView.ItemsSource = App.CustomMiBand.DisplayItems.Where(Function(x) x.IsEnabled = True).OrderBy(Function(x) x.Operation)

            If Await RegisterTaskForNotifications() Then
                Debug.WriteLine($"BackgroundTask for Notifications running!")
            Else
                Debug.WriteLine($"BackgroundTask for Notifications NOT running!")
            End If

            If Await RegisterTaskForSync() Then
                Debug.WriteLine($"BackgroundTask for Sync running!")
            Else
                Debug.WriteLine($"BackgroundTask for Sync NOT running!")
            End If

            ' Update List
            btnSync_Click(sender, e)

            ' Check Rights
            Await GetNotificationsListenerAccess()

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
        End Try
    End Sub

    Private Async Function GetNotificationsListenerAccess() As Task(Of Boolean)
        Dim Listener As UserNotificationListener = Nothing
        Dim AccessStatus As UserNotificationListenerAccessStatus = Nothing
        Try
            Listener = UserNotificationListener.Current
            AccessStatus = Await Listener.RequestAccessAsync
            Select Case AccessStatus
                Case UserNotificationListenerAccessStatus.Allowed
                    Return True
                Case UserNotificationListenerAccessStatus.Denied, UserNotificationListenerAccessStatus.Unspecified
                    Return False
            End Select

            Return False

        Catch ex As Exception
            Return False
        Finally
            Listener = Nothing
            AccessStatus = Nothing
        End Try
    End Function

    Private Sub Frame_DataContextChanged(sender As FrameworkElement, args As DataContextChangedEventArgs)
        If TryCast(args.NewValue, CustomMiBandResult) IsNot Nothing Then
            Select Case DirectCast(args.NewValue, CustomMiBandResult).Operation
                Case CustomMiBandResult.BandOperation.Battery
                    DirectCast(sender, Frame).Navigate(GetType(BatteryPage), App.CustomMiBand.BatteryResult)
                Case CustomMiBandResult.BandOperation.Notifications
                    DirectCast(sender, Frame).Navigate(GetType(NotificationPage), App.CustomMiBand.NotificationResult)
                Case CustomMiBandResult.BandOperation.Calories, CustomMiBandResult.BandOperation.Steps
                    DirectCast(sender, Frame).Navigate(GetType(StepsPage), App.CustomMiBand.StepResult)
                Case CustomMiBandResult.BandOperation.Distance
                    DirectCast(sender, Frame).Navigate(GetType(DistancePage), App.CustomMiBand.StepResult)
                Case CustomMiBandResult.BandOperation.Heartrate
                    DirectCast(sender, Frame).Navigate(GetType(HeartratePage), App.CustomMiBand.HeartResult)
                Case CustomMiBandResult.BandOperation.Welcome
                    DirectCast(sender, Frame).Navigate(GetType(WelcomePage))
                Case Else

            End Select
        End If
    End Sub

    Private Sub btnSetting_Click(sender As Object, e As RoutedEventArgs)
        Frame.Navigate(GetType(SettingsPage))
    End Sub

    Private Sub btnProfile_Click(sender As Object, e As RoutedEventArgs)
        Frame.Navigate(GetType(Profile))
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        If e.SourcePageType Is GetType(WelcomePage) Then
            btnSync_Click(Nothing, Nothing)
        End If
    End Sub
End Class
