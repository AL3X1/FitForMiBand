' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

Imports ClassesCollection
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class Profile
    Inherits Page

    Public Sub New()

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Dim Heights As New List(Of CustomComboBoxItem)
        For i = 80 To 240
            Heights.Add(New CustomComboBoxItem(i.ToString & " cm", i.ToString))
        Next
        Me.cboHeight.ItemsSource = Heights
        Me.cboHeight.DisplayMemberPath = "Text"

        Dim Weights As New List(Of CustomComboBoxItem)
        For i = 30 To 170
            Weights.Add(New CustomComboBoxItem(i.ToString & " kg", i.ToString))
        Next
        Me.cboWeight.ItemsSource = Weights
        Me.cboWeight.DisplayMemberPath = "Text"

        Dim Steps As New List(Of CustomComboBoxItem)
        For i = 2000 To 50000 Step 1000
            Steps.Add(New CustomComboBoxItem(i.ToString & " steps", i.ToString))
        Next
        Me.cboSteps.ItemsSource = Steps
        Me.cboSteps.DisplayMemberPath = "Text"

        If App.LocalSettings.Values("Profile_Alias") IsNot Nothing Then
            Me.txtAlias.Text = App.LocalSettings.Values("Profile_Alias").ToString
        End If

        If App.LocalSettings.Values("Profile_Gender") IsNot Nothing Then
            cboGender.SelectedValue = App.LocalSettings.Values("Profile_Gender")
        End If

        If App.LocalSettings.Values("Profile_Height") IsNot Nothing Then
            cboHeight.SelectedItem = Heights.FirstOrDefault(Function(x) x.Value = App.LocalSettings.Values("Profile_Height").ToString)
        End If

        If App.LocalSettings.Values("Profile_Weight") IsNot Nothing Then
            cboWeight.SelectedItem = Weights.FirstOrDefault(Function(x) x.Value = App.LocalSettings.Values("Profile_Weight").ToString)
        End If

        If App.LocalSettings.Values("Profile_Steps") IsNot Nothing Then
            cboSteps.SelectedItem = Steps.FirstOrDefault(Function(x) x.Value = App.LocalSettings.Values("Profile_Steps").ToString)
        End If

        If App.LocalSettings.Values("Profile_DateOfBirth") IsNot Nothing Then
            dtpDateOfBirth.Date = CDate(App.LocalSettings.Values("Profile_DateOfBirth"))
        End If

        If App.LocalSettings.Values("Profile_Sleep") IsNot Nothing Then
            dtpSleep.Time = TimeSpan.Parse(App.LocalSettings.Values("Profile_Sleep").ToString)
        End If

    End Sub

    Private Sub cboGender_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboGender.SelectionChanged
        If cboGender.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Gender") = cboGender.SelectedItem
        End If
    End Sub

    Private Sub cboHeight_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboHeight.SelectionChanged
        If cboHeight.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Height") = DirectCast(cboHeight.SelectedItem, CustomComboBoxItem).Value
        End If
    End Sub

    Private Sub cboSteps_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboSteps.SelectionChanged
        If cboSteps.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Steps") = DirectCast(cboSteps.SelectedItem, CustomComboBoxItem).Value
        End If
    End Sub

    Private Sub cboWeight_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cboWeight.SelectionChanged
        If cboWeight.SelectedItem IsNot Nothing Then
            App.LocalSettings.Values("Profile_Weight") = DirectCast(cboWeight.SelectedItem, CustomComboBoxItem).Value
        End If
    End Sub

    Private Sub dtpDateOfBirth_DateChanged(sender As Object, e As DatePickerValueChangedEventArgs) Handles dtpDateOfBirth.DateChanged
        App.LocalSettings.Values("Profile_DateOfBirth") = dtpDateOfBirth.Date.ToString
    End Sub

    Private Sub dtpSleep_TimeChanged(sender As Object, e As TimePickerValueChangedEventArgs) Handles dtpSleep.TimeChanged
        App.LocalSettings.Values("Profile_Sleep") = dtpSleep.Time.ToString
    End Sub

    Private Sub txtAlias_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtAlias.TextChanged
        App.LocalSettings.Values("Profile_Alias") = Me.txtAlias.Text
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As RoutedEventArgs)
        pbProcessing.Visibility = Visibility.Visible
        btnSave.IsEnabled = False
        Try
            Await App.CustomMiBand.setUserInfo(CType(App.LocalSettings.Values("Profile_Alias"), String), dtpDateOfBirth.Date.Date, CType(App.LocalSettings.Values("Profile_Gender"), String), CInt(App.LocalSettings.Values("Profile_Height")), CInt(App.LocalSettings.Values("Profile_Weight")))

            If Frame.CanGoBack Then
                Frame.GoBack()
            End If

        Catch ex As Exception
            Helpers.DebugWriter(GetType(Profile), ex.Message)
        Finally
            pbProcessing.Visibility = Visibility.Collapsed
            btnSave.IsEnabled = True
        End Try
    End Sub
End Class
