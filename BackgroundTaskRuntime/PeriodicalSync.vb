Imports ClassesCollection
Imports Windows.ApplicationModel.Background

Public NotInheritable Class PeriodicalSync
    Implements IBackgroundTask

    Private _deferral As BackgroundTaskDeferral
    Private _customMiBand As CustomMiBand

    Public Async Sub Run(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run
        _deferral = taskInstance.GetDeferral()
        _customMiBand = Nothing
        Try
            Debug.WriteLine($"MESSAGE: BackgroundTask: {GetType(NotificationChanged).FullName} running!")

            _customMiBand = New CustomMiBand
            Await _customMiBand.UpdateOperations()


        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
        Finally
            If _customMiBand IsNot Nothing Then _customMiBand.Dispose()
            _customMiBand = Nothing

            _deferral.Complete()
        End Try
    End Sub
End Class
