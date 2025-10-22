Imports System.Security.Cryptography
Imports System.Text
Imports System.Data.OleDb

Public Class frmResetPassword
    Private _userID As Integer
    Private _username As String
    Dim con As OleDbConnection
    Dim cmd As OleDbCommand
    Private isClosing As Boolean = False

    Public Sub New(userID As Integer, username As String)
        InitializeComponent()
        _userID = userID
        _username = username
        lblUsername.Text = "Resetting password for: " & _username
        con = DatabaseConfig.GetConnection()
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        If isClosing Then Return

        If ValidateInput() Then
            Try
                If con.State = ConnectionState.Open Then con.Close()
                con.Open()

                Dim hashedPassword As String = HashPassword(txtPassword.Text)

                ' FIXED: Use [UserPassword] instead of Password (reserved word in Access)
                cmd = New OleDbCommand("UPDATE tblUsers SET [UserPassword]=? WHERE UserID=?", con)
                cmd.Parameters.AddWithValue("@1", hashedPassword)
                cmd.Parameters.AddWithValue("@2", _userID)
                cmd.ExecuteNonQuery()

                MessageBox.Show("Password reset successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.DialogResult = DialogResult.OK

                ' Clean close
                isClosing = True
                Me.Close()

            Catch ex As Exception
                If Not isClosing Then
                    MessageBox.Show("Error resetting password: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            Finally
                Try
                    If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                        con.Close()
                    End If
                Catch
                End Try
            End Try
        End If
    End Sub

    Private Sub chkShowPassword_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowPassword.CheckedChanged
        If isClosing Then Return

        If chkShowPassword.Checked Then
            txtPassword.PasswordChar = ControlChars.NullChar
            txtConfirmPassword.PasswordChar = ControlChars.NullChar
        Else
            txtPassword.PasswordChar = "●"c
            txtConfirmPassword.PasswordChar = "●"c
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        isClosing = True
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub btnGeneratePassword_Click(sender As Object, e As EventArgs) Handles btnGeneratePassword.Click
        If isClosing Then Return

        Dim randomPassword As String = GenerateRandomPassword(10)
        txtPassword.Text = randomPassword
        txtConfirmPassword.Text = randomPassword
        MessageBox.Show("Generated password: " & randomPassword & vbCrLf & vbCrLf & "Please save this password securely.", "Generated Password", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Function ValidateInput() As Boolean
        If String.IsNullOrWhiteSpace(txtPassword.Text) Then
            MessageBox.Show("Please enter a new password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPassword.Focus()
            Return False
        End If

        If txtPassword.Text.Length < 6 Then
            MessageBox.Show("Password must be at least 6 characters long.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPassword.Focus()
            Return False
        End If

        If txtPassword.Text <> txtConfirmPassword.Text Then
            MessageBox.Show("Passwords do not match!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtConfirmPassword.Focus()
            Return False
        End If

        Return True
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

    ' Modern random password generator
    Private Function GenerateRandomPassword(length As Integer) As String
        Const chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()"
        Dim result As New StringBuilder(length)
        Dim buffer As Byte() = New Byte(0) {}

        For i As Integer = 1 To length
            buffer = New Byte(0) {}
            RandomNumberGenerator.Fill(buffer)
            Dim idx As Integer = buffer(0) Mod chars.Length
            result.Append(chars(idx))
        Next

        Return result.ToString()
    End Function

    ' === Form Closing - Prevent COM issues ===
    Private Sub frmResetPassword_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            isClosing = True

            ' Close connection if open
            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
            End Try

            ' Remove event handlers
            Try
                RemoveHandler btnReset.Click, AddressOf btnReset_Click
                RemoveHandler btnCancel.Click, AddressOf btnCancel_Click
                RemoveHandler btnGeneratePassword.Click, AddressOf btnGeneratePassword_Click
                RemoveHandler chkShowPassword.CheckedChanged, AddressOf chkShowPassword_CheckedChanged
            Catch
            End Try

        Catch ex As Exception
            Debug.WriteLine("ResetPassword FormClosing error: " & ex.Message)
        End Try
    End Sub

    ' === Form Closed ===
    Private Sub frmResetPassword_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            ' Dispose connection
            If con IsNot Nothing Then
                Try
                    con.Dispose()
                Catch
                End Try
                con = Nothing
            End If

            GC.SuppressFinalize(Me)
        Catch
        End Try
    End Sub
End Class