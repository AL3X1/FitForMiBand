Imports ClassesCollection
Imports Windows.UI.Core
Imports Windows.UI.Notifications
Imports Windows.UI.Notifications.Management
''' <summary>
''' Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
''' </summary>
NotInheritable Class App
    Inherits Application

    Public Shared LocalSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings
    Public Shared CustomMiBand As CustomMiBand

    ''' <summary>
    ''' Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
    ''' werden verwendet, wenn die Anwendung zum Öffnen einer bestimmten Datei, zum Anzeigen
    ''' von Suchergebnissen usw. gestartet wird.
    ''' </summary>
    ''' <param name="e">Details über Startanforderung und -prozess.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
        ' Nur sicherstellen, dass das Fenster aktiv ist.

        If rootFrame Is Nothing Then
            ' Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed
            AddHandler rootFrame.Navigated, AddressOf OnNavigated

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Zustand von zuvor angehaltener Anwendung laden
            End If
            ' Den Frame im aktuellen Fenster platzieren
            Window.Current.Content = rootFrame

            AddHandler SystemNavigationManager.GetForCurrentView.BackRequested, AddressOf OnBackRequested

            If rootFrame.CanGoBack Then
                SystemNavigationManager.GetForCurrentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible
            Else
                SystemNavigationManager.GetForCurrentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed
            End If

        End If

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                ' und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                ' übergeben werden
                rootFrame.Navigate(GetType(Splash), e.Arguments)
            End If

            ' Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
    ''' </summary>
    ''' <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
    ''' <param name="e">Details über den Navigationsfehler</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    Private Sub OnNavigated(sender As Object, e As NavigationEventArgs)
        If e.SourcePageType.ToString.Contains("MainPage") Then DirectCast(sender, Frame).BackStack.Clear()
        Debug.WriteLine($"NAVIGATION {e.SourcePageType.ToString}")

        ' Each time a navigation event occurs, update the Back button visibility
        If DirectCast(sender, Frame).CanGoBack Then
            SystemNavigationManager.GetForCurrentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible
        Else
            SystemNavigationManager.GetForCurrentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed
        End If
    End Sub

    Private Sub OnBackRequested(sender As Object, e As BackRequestedEventArgs)
        Dim rootFrame As Frame = CType(Window.Current.Content, Frame)
        If rootFrame.CanGoBack Then
            e.Handled = True
            rootFrame.GoBack()
        End If
    End Sub

    ''' <summary>
    ''' Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
    ''' ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
    ''' unbeschädigt bleiben.
    ''' </summary>
    ''' <param name="sender">Die Quelle der Anhalteanforderung.</param>
    ''' <param name="e">Details zur Anhalteanforderung.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()

        If CustomMiBand IsNot Nothing Then
            CustomMiBand.Dispose()
            CustomMiBand = Nothing
        End If

        deferral.Complete()
    End Sub

End Class