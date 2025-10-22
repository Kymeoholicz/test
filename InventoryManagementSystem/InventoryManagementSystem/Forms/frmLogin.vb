Imports System.Data.OleDb
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO

Public Class frmLogin
    Dim con As OleDbConnection
    Dim cmd As OleDbCommand
    Dim dr As OleDbDataReader
    Private loginAttempts As Integer = 0
    Private Const MaxLoginAttempts As Integer = 3

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.CenterToScreen()
        Me.Text = "Login - Inventory Management System"
        txtPassword.PasswordChar = "●"
        txtUsername.Focus()

        Me.AcceptButton = btnLogin

        ' Initialize database
        InitializeDatabase()
    End Sub

    Private Sub InitializeDatabase()
        Try
            ' Initialize connection string
            DatabaseConfig.InitializeConnectionString()
            con = DatabaseConfig.GetConnection()

            Dim dbPath As String = DatabaseConfig.GetDatabasePath()

            ' Check if database file exists
            If Not File.Exists(dbPath) Then
                Dim result = MessageBox.Show("Database not found!" & vbCrLf & vbCrLf &
                    "Expected location: " & dbPath & vbCrLf & vbCrLf &
                    "Would you like to create a new database with default tables and admin account?",
                    "Database Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                If result = DialogResult.Yes Then
                    ' Create empty database file using ADOX
                    Try
                        Dim cat As Object = CreateObject("ADOX.Catalog")
                        cat.Create(DatabaseConfig.ConnectionString)
                        cat = Nothing

                        ' Create tables
                        DatabaseConfig.CreateTables()
                        DatabaseConfig.CreateDefaultAdmin()
                    Catch ex As Exception
                        MessageBox.Show("Error creating database: " & ex.Message & vbCrLf & vbCrLf &
                            "Please create 'inventory.accdb' manually using Microsoft Access and run the application again.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Application.Exit()
                    End Try
                Else
                    Application.Exit()
                End If
            Else
                ' Verify database structure
                DatabaseConfig.VerifyDatabaseStructure()
            End If

            ' Test connection
            If Not DatabaseConfig.TestConnection() Then
                MessageBox.Show("Cannot connect to database!" & vbCrLf &
                    "Path: " & dbPath & vbCrLf & vbCrLf &
                    "Please ensure Microsoft Access Database Engine is installed.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                ' Check and fix database structure if needed
                DatabaseFixHelper.FixUserTableStructure()
            End If
        Catch ex As Exception
            MessageBox.Show("Database initialization error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        If String.IsNullOrWhiteSpace(txtUsername.Text) Then
            MessageBox.Show("Please enter username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtUsername.Focus()
            Return
        End If

        If String.IsNullOrWhiteSpace(txtPassword.Text) Then
            MessageBox.Show("Please enter password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPassword.Focus()
            Return
        End If

        Try
            If AuthenticateUser(txtUsername.Text.Trim(), txtPassword.Text) Then
                MessageBox.Show("Login successful! Welcome " & CurrentUser.FullName, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                LogActivity("Login", "User logged in successfully")

                ' Show main menu and handle logout loop
                Do
                    Dim mainMenu As New frmMainMenu()
                    Dim result As DialogResult = DialogResult.None

                    Try
                        result = mainMenu.ShowDialog()
                    Catch ex As Exception
                        Debug.WriteLine($"Main Menu error: {ex.GetType().Name} - {ex.Message}")
                        result = DialogResult.Cancel
                    Finally
                        ' Dispose immediately after closing
                        Try
                            mainMenu.Dispose()
                        Catch
                        End Try
                    End Try

                    ' If user logged out (DialogResult.Retry), show login again
                    ' Otherwise (Exit or closed), exit the loop
                    If result <> DialogResult.Retry Then
                        Exit Do
                    End If

                    ' User logged out, clear fields and stay on login
                    txtUsername.Clear()
                    txtPassword.Clear()
                    txtUsername.Focus()
                    loginAttempts = 0 ' Reset login attempts
                    Return ' Stay on login form

                Loop

                ' User exited, close login form
                Me.Close()
            Else
                loginAttempts += 1
                Dim remainingAttempts As Integer = MaxLoginAttempts - loginAttempts

                If remainingAttempts > 0 Then
                    MessageBox.Show("Invalid username or password!" & vbCrLf & "Attempts remaining: " & remainingAttempts, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    txtPassword.Clear()
                    txtPassword.Focus()
                Else
                    MessageBox.Show("Maximum login attempts exceeded. Application will close.", "Security", MessageBoxButtons.OK, MessageBoxIcon.Stop)
                    Environment.Exit(0)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show($"Login error: {ex.GetType().Name}" & vbCrLf & vbCrLf &
                          $"Message: {ex.Message}" & vbCrLf & vbCrLf &
                          $"Details: {ex.StackTrace}",
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Environment.Exit(1)
        End Try
    End Sub

    Private Function AuthenticateUser(username As String, password As String) As Boolean
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            ' Hash the input password before comparing
            Dim hashedPassword As String = HashPassword(password)

            ' FIXED: Use [UserPassword] instead of Password (reserved word in Access)
            cmd = New OleDbCommand("SELECT UserID, Username, FullName, UserRole, IsActive FROM tblUsers WHERE Username = ? AND [UserPassword] = ?", con)

            cmd.Parameters.AddWithValue("@1", username)
            cmd.Parameters.AddWithValue("@2", hashedPassword)

            dr = cmd.ExecuteReader()

            If dr.Read() Then
                ' FIXED: Handle IsActive as various possible types (Boolean, Integer, String)
                Dim isActiveValue As Object = dr("IsActive")
                Dim isActive As Boolean = False

                If IsDBNull(isActiveValue) Then
                    isActive = True ' Default to active if null
                ElseIf TypeOf isActiveValue Is Boolean Then
                    isActive = CBool(isActiveValue)
                ElseIf TypeOf isActiveValue Is Integer Then
                    isActive = (CInt(isActiveValue) <> 0)
                ElseIf TypeOf isActiveValue Is String Then
                    isActive = (isActiveValue.ToString().ToLower() = "true" Or isActiveValue.ToString() = "1" Or isActiveValue.ToString().ToLower() = "yes")
                Else
                    isActive = True ' Default to active for unknown types
                End If

                If Not isActive Then
                    MessageBox.Show("Your account has been deactivated.", "Account Inactive", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return False
                End If

                ' Assign user details to CurrentUser object
                CurrentUser.UserID = CInt(dr("UserID"))
                CurrentUser.Username = If(IsDBNull(dr("Username")), "", dr("Username").ToString())
                CurrentUser.FullName = If(IsDBNull(dr("FullName")), "", dr("FullName").ToString())
                CurrentUser.UserRole = If(IsDBNull(dr("UserRole")), "User", dr("UserRole").ToString())
                CurrentUser.IsLoggedIn = True

                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            MessageBox.Show("Login error: " & ex.Message & vbCrLf & vbCrLf & "Stack Trace: " & ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Finally
            If dr IsNot Nothing Then
                Try
                    dr.Close()
                Catch
                End Try
            End If
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Function

    Private Function HashPassword(password As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
            Dim builder As New StringBuilder()
            For Each b As Byte In bytes
                builder.Append(b.ToString("x2"))
            Next
            Return builder.ToString()
        End Using
    End Function

    Private Sub LogActivity(activityType As String, description As String)
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            cmd = New OleDbCommand("INSERT INTO tblActivityLog (UserID, ActivityType, Description, ActivityDate) VALUES (?,?,?,?)", con)
            cmd.Parameters.AddWithValue("@1", CurrentUser.UserID)
            cmd.Parameters.AddWithValue("@2", activityType)
            cmd.Parameters.AddWithValue("@3", description)
            cmd.Parameters.AddWithValue("@4", Date.Now)
            cmd.ExecuteNonQuery()
        Catch ex As Exception
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        If MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Application.Exit()
        End If
    End Sub

    Private Sub chkShowPassword_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowPassword.CheckedChanged
        If chkShowPassword.Checked Then
            txtPassword.PasswordChar = ""
        Else
            txtPassword.PasswordChar = "●"
        End If
    End Sub

    Private Sub txtUsername_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUsername.KeyPress
        If e.KeyChar = " " Then
            e.Handled = True
        End If
    End Sub

    Private Sub lblForgotPassword_Click(sender As Object, e As EventArgs) Handles lblForgotPassword.Click
        MessageBox.Show("Please contact your system administrator to reset your password.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnDatabaseInfo_Click(sender As Object, e As EventArgs) Handles btnDatabaseInfo.Click
        DatabaseConfig.ShowDatabaseInfo()
    End Sub
End Class