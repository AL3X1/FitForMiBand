Imports ClassesCollection
Imports Windows.Storage.Streams

Public Class GetAppLogo
    Implements IValueConverter

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim _bitmap As New BitmapImage
        Dim _task As Task(Of IRandomAccessStream) = Nothing
        Try
            _task = Task.Run(Async Function()
                                 Return Await Helpers.GetAppLogoById(value.ToString)
                             End Function)
            _task.Wait()

            _bitmap.SetSource(_task.Result)

            Return _bitmap

        Catch ex As Exception
            Return Nothing
        Finally
            _task = Nothing
            _bitmap = Nothing
        End Try
    End Function
End Class
