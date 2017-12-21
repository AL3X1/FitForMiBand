' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class ClockPage
    Inherits Page

    Public Sub New()

        InitializeComponent()

        Me.lblNow.Text = String.Format("{0}", DateTime.Now.ToString("dddd, MM/dd/yyyy HH:mm"))
        Me.lblThisDevice.Text = String.Format("This Device: {0}", DateTime.Now.ToString("dddd, MM/dd/yyyy HH:mm"))
        Me.lblLastSync.Text = String.Format("Sync date: {0}", DateTime.Now.ToString("dddd, MM/dd/yyyy HH:mm"))
        Me.lblOnBand.Text = String.Format("On Band: {0}", DateTime.Now.ToString("dddd, MM/dd/yyyy HH:mm"))
    End Sub

    Private Sub btnSync_Click(sender As Object, e As RoutedEventArgs)

    End Sub
End Class
