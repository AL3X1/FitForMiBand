Imports Windows.ApplicationModel.Background
Imports Windows.UI.Notifications

Public NotInheritable Class NotificationHistoryChangedTriggerTask
    Implements IBackgroundTask

    Private _deferral As BackgroundTaskDeferral

    Public Sub Run(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run

        _deferral = taskInstance.GetDeferral()
        AddHandler taskInstance.Canceled, Sub()
                                              _deferral.Complete()
                                          End Sub
        Try
            Dim details = taskInstance.TriggerDetails

        Catch ex As Exception
        Finally
            _deferral.Complete()
        End Try
    End Sub
End Class
