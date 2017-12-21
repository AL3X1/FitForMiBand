Imports ClassesCollection
Imports Windows.ApplicationModel.Background
Imports Windows.UI.Notifications
Imports Windows.UI.Notifications.Management

Public NotInheritable Class NotificationChanged
    Implements IBackgroundTask

    Private _deferral As BackgroundTaskDeferral
    Private _customMiBand As CustomMiBand
    Private _userNotifListener As UserNotificationListener
    Private _singleNotification As NotificationRequest
    Private _toastBinding As NotificationBinding


    Public Async Sub RunAsync(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run
        _deferral = taskInstance.GetDeferral()
        _customMiBand = Nothing
        Try
            Debug.WriteLine($"MESSAGE: BackgroundTask: {GetType(NotificationChanged).FullName} running!")

            _userNotifListener = UserNotificationListener.Current
            _customMiBand = New CustomMiBand

            If _userNotifListener.GetAccessStatus = UserNotificationListenerAccessStatus.Allowed Then

                ' Get all notifications
                Dim UserNotifications As IReadOnlyList(Of UserNotification) = Await _userNotifListener.GetNotificationsAsync(NotificationKinds.Toast)

                ' Store Notification of any App in App-List if its a new App

                ' Get Last Notification from List
                Dim UserNotification = UserNotifications.LastOrDefault

                If UserNotification IsNot Nothing Then
                    If UserNotification.AppInfo IsNot Nothing Then

                        ' Wenn Nachricht älter als 15 Sekunden, dann vergessen
                        Dim _validityTime As TimeSpan = DateTime.Now - UserNotification.CreationTime
                        If _validityTime.TotalSeconds < 5 Then

                            If _customMiBand.NotificationResult.Requests.Where(Function(x) x.Id = UserNotification.AppInfo.AppUserModelId).Count = 0 Then
                                If UserNotification.AppInfo IsNot Nothing Then
                                    _customMiBand.NotificationResult.Add(UserNotification.AppInfo)
                                End If
                            End If

                            ' Get NotificationRequest infos
                            _singleNotification = _customMiBand.NotificationResult.Requests.FirstOrDefault(Function(x) x.Id = UserNotification.AppInfo.AppUserModelId)
                            If _singleNotification IsNot Nothing Then

                                ' Check if Notifications for that App is active
                                If _singleNotification.IsOn = True Then

                                    ' Get Title and Text of Notification
                                    Dim title As String = ""
                                    Dim text As String = ""

                                    _toastBinding = UserNotification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric)
                                    If _toastBinding IsNot Nothing Then
                                        If _toastBinding.GetTextElements().Count > 0 Then
                                            title = _toastBinding.GetTextElements().FirstOrDefault.Text
                                            text = String.Join("\n", _toastBinding.GetTextElements().Skip(1).Select(Function(x) x.Text).ToArray)
                                        End If
                                    End If

                                    ' Send Notification to Band
                                    Await _customMiBand.SetNewAlert(_singleNotification, title, text)

                                End If
                            End If
                        End If
                        'Next
                    End If
                End If
            End If

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
        Finally
            If _customMiBand IsNot Nothing Then _customMiBand.Dispose()
            _customMiBand = Nothing
            _userNotifListener = Nothing
            _singleNotification = Nothing
            _toastBinding = Nothing

            _deferral.Complete()
        End Try
    End Sub
End Class
