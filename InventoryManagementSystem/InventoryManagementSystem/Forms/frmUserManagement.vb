Imports System.Data.OleDb
Imports System.Security.Cryptography
Imports System.Text

Public Class frmUserManagement
    Dim con As OleDbConnection
    Dim cmd As OleDbCommand
    Dim da As OleDbDataAdapter
    Dim dt As DataTable
    Dim selectedUserID As Integer = 0

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub frmUserManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Only admins can manage users
        If Not CurrentUser.IsAdmin() Then
            MessageBox.Show("Access Denied! Only administrators can manage users.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Me.Close()
            Return
        End If

        con = DatabaseConfig.GetConnection()
        LoadRoles()
        LoadData()
        ClearFields()
        Me.Text = "User Management - Admin Only"
        Me.WindowState = FormWindowState.Maximized
    End Sub

    '==========================
    ' LOAD & INITIALIZATION
    '==========================
    Private Sub LoadRoles()
        cmbUserRole.Items.Clear()
        cmbUserRole.Items.Add("Admin")
        cmbUserRole.Items.Add("Manager")
        cmbUserRole.Items.Add("User")
        cmbUserRole.SelectedIndex = 2

        ' Also load role filter
        cmbRoleFilter.Items.Clear()
        cmbRoleFilter.Items.Add("All Roles")
        cmbRoleFilter.Items.Add("Admin")
        cmbRoleFilter.Items.Add("Manager")
        cmbRoleFilter.Items.Add("User")
        cmbRoleFilter.SelectedIndex = 0
    End Sub

    Private Sub LoadData()
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            ' FIXED: Cast IsActive properly for display
            da = New OleDbDataAdapter("SELECT UserID AS [ID], Username, FullName AS [Full Name], Email, UserRole AS [Role], IIF(IsActive, 'Yes', 'No') AS [Active], Format(DateCreated, 'yyyy-MM-dd') AS [Created] FROM tblUsers ORDER BY UserID DESC", con)
            dt = New DataTable
            da.Fill(dt)
            DataGridView1.DataSource = dt

            If DataGridView1.Columns.Count > 0 Then
                DataGridView1.Columns(0).Width = 50
                DataGridView1.Columns(1).Width = 120
                DataGridView1.Columns(2).Width = 180
                DataGridView1.Columns(3).Width = 200
                DataGridView1.Columns(4).Width = 80
                DataGridView1.Columns(5).Width = 70
                DataGridView1.Columns(6).Width = 100
            End If

            lblTotalUsers.Text = "Total Users: " & dt.Rows.Count.ToString()

            ' Calculate stats safely
            Dim admins As Integer = 0
            Dim managers As Integer = 0
            Dim users As Integer = 0

            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row("Role")) Then
                    Dim role As String = row("Role").ToString()
                    Select Case role.ToLower()
                        Case "admin"
                            admins += 1
                        Case "manager"
                            managers += 1
                        Case "user"
                            users += 1
                    End Select
                End If
            Next

            lblUserStats.Text = $"Admins: {admins} | Managers: {managers} | Users: {users}"
        Catch ex As Exception
            MessageBox.Show("Error loading data: " & ex.Message & vbCrLf & vbCrLf & ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' CLEAR FIELDS
    '==========================
    Private Sub ClearFields()
        txtUsername.Clear()
        txtFullName.Clear()
        txtEmail.Clear()
        txtPassword.Clear()
        txtConfirmPassword.Clear()
        If cmbUserRole.Items.Count > 0 Then cmbUserRole.SelectedIndex = 2
        chkIsActive.Checked = True
        selectedUserID = 0
        btnAdd.Enabled = True
        btnUpdate.Enabled = False
        btnDelete.Enabled = False
        btnResetPassword.Enabled = False
        txtUsername.Enabled = True
        txtPassword.Enabled = True
        txtConfirmPassword.Enabled = True
        lblPasswordNote.Text = "Password must be at least 6 characters"
        lblPasswordNote.ForeColor = Color.Gray
    End Sub

    '==========================
    ' PASSWORD HASHING
    '==========================
    Private Function HashPassword(password As String) As String
        Using sha As SHA256 = SHA256.Create()
            Dim bytes = Encoding.UTF8.GetBytes(password)
            Dim hash = sha.ComputeHash(bytes)
            Return BitConverter.ToString(hash).Replace("-", "").ToLower()
        End Using
    End Function

    '==========================
    ' ADD USER
    '==========================
    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If txtUsername.Text.Trim() = "" OrElse txtFullName.Text.Trim() = "" OrElse txtPassword.Text.Trim() = "" Then
            MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If txtPassword.Text <> txtConfirmPassword.Text Then
            MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        If txtPassword.TextLength < 6 Then
            MessageBox.Show("Password must be at least 6 characters long.", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            ' Check if username exists
            cmd = New OleDbCommand("SELECT COUNT(*) FROM tblUsers WHERE Username = ?", con)
            cmd.Parameters.AddWithValue("@1", txtUsername.Text.Trim())
            Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            If count > 0 Then
                MessageBox.Show("Username already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' FIXED: Removed CreatedBy field (may not exist or cause type mismatch)
            cmd = New OleDbCommand("INSERT INTO tblUsers (Username, FullName, Email, [UserPassword], UserRole, IsActive, DateCreated) VALUES (?,?,?,?,?,?,?)", con)
            cmd.Parameters.Add("@1", OleDbType.VarWChar, 50).Value = txtUsername.Text.Trim()
            cmd.Parameters.Add("@2", OleDbType.VarWChar, 100).Value = txtFullName.Text.Trim()
            cmd.Parameters.Add("@3", OleDbType.VarWChar, 100).Value = If(String.IsNullOrWhiteSpace(txtEmail.Text), "", txtEmail.Text.Trim())
            cmd.Parameters.Add("@4", OleDbType.VarWChar, 255).Value = HashPassword(txtPassword.Text.Trim())
            cmd.Parameters.Add("@5", OleDbType.VarWChar, 20).Value = cmbUserRole.Text
            cmd.Parameters.Add("@6", OleDbType.Boolean).Value = chkIsActive.Checked
            cmd.Parameters.Add("@7", OleDbType.Date).Value = Date.Now
            cmd.ExecuteNonQuery()

            LogActivity("Add User", "Added user: " & txtUsername.Text)
            MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            LoadData()
            ClearFields()
        Catch ex As Exception
            MessageBox.Show("Error adding user: " & ex.Message & vbCrLf & vbCrLf & "Details: " & ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' UPDATE USER
    '==========================
    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        If selectedUserID = 0 Then
            MessageBox.Show("Select a user to update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            cmd = New OleDbCommand("UPDATE tblUsers SET FullName=?, Email=?, UserRole=?, IsActive=? WHERE UserID=?", con)
            cmd.Parameters.AddWithValue("@1", txtFullName.Text.Trim())
            cmd.Parameters.AddWithValue("@2", txtEmail.Text.Trim())
            cmd.Parameters.AddWithValue("@3", cmbUserRole.Text)
            cmd.Parameters.AddWithValue("@4", chkIsActive.Checked)
            cmd.Parameters.AddWithValue("@5", selectedUserID)
            cmd.ExecuteNonQuery()

            LogActivity("Update User", "Updated user: " & txtUsername.Text)
            MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            LoadData()
            ClearFields()
        Catch ex As Exception
            MessageBox.Show("Error updating user: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' DELETE USER
    '==========================
    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If selectedUserID = 0 Then
            MessageBox.Show("Select a user to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If selectedUserID = CurrentUser.UserID Then
            MessageBox.Show("You cannot delete your own account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim confirm = MessageBox.Show("Are you sure you want to delete user: " & txtUsername.Text & "?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If confirm = DialogResult.No Then Return

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            cmd = New OleDbCommand("DELETE FROM tblUsers WHERE UserID=?", con)
            cmd.Parameters.AddWithValue("@1", selectedUserID)
            cmd.ExecuteNonQuery()

            LogActivity("Delete User", "Deleted user: " & txtUsername.Text)
            MessageBox.Show("User deleted successfully!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information)

            LoadData()
            ClearFields()
        Catch ex As Exception
            MessageBox.Show("Error deleting user: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' RESET PASSWORD
    '==========================
    Private Sub btnResetPassword_Click(sender As Object, e As EventArgs) Handles btnResetPassword.Click
        If selectedUserID > 0 Then
            Dim resetForm As frmResetPassword = Nothing
            Try
                resetForm = New frmResetPassword(selectedUserID, txtUsername.Text)
                Dim result As DialogResult = resetForm.ShowDialog()

                If result = DialogResult.OK Then
                    LogActivity("Reset Password", "Reset password for user: " & txtUsername.Text)
                    MessageBox.Show("Password reset successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            Catch ex As Exception
                MessageBox.Show("Error opening reset password form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                ' Always dispose the form
                If resetForm IsNot Nothing Then
                    Try
                        resetForm.Dispose()
                    Catch
                    End Try
                    resetForm = Nothing
                End If
            End Try
        Else
            MessageBox.Show("Please select a user first.", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    '==========================
    ' SEARCH
    '==========================
    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Dim searchTerm As String = txtSearch.Text.Trim()

        If searchTerm = "" Then
            LoadData()
            Return
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim query As String = "SELECT UserID AS [ID], Username, FullName AS [Full Name], Email, UserRole AS [Role], IsActive AS [Active], Format(DateCreated, 'yyyy-MM-dd') AS [Created] FROM tblUsers WHERE Username LIKE ? OR FullName LIKE ? OR Email LIKE ? ORDER BY UserID DESC"

            da = New OleDbDataAdapter(query, con)
            Dim searchPattern As String = "%" & searchTerm & "%"
            da.SelectCommand.Parameters.AddWithValue("@1", searchPattern)
            da.SelectCommand.Parameters.AddWithValue("@2", searchPattern)
            da.SelectCommand.Parameters.AddWithValue("@3", searchPattern)

            dt = New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt

            lblTotalUsers.Text = "Search Results: " & dt.Rows.Count.ToString()
        Catch ex As Exception
            MessageBox.Show("Error searching: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' REFRESH
    '==========================
    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            txtSearch.Clear()
            LoadData()
            ClearFields()
            MessageBox.Show("User list refreshed successfully.", "Refreshed", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error refreshing data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================
    ' DATAGRID CLICK
    '==========================
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex >= 0 Then
            Try
                selectedUserID = Convert.ToInt32(DataGridView1.Rows(e.RowIndex).Cells("ID").Value)
                txtUsername.Text = DataGridView1.Rows(e.RowIndex).Cells("Username").Value.ToString()
                txtFullName.Text = DataGridView1.Rows(e.RowIndex).Cells("Full Name").Value.ToString()
                txtEmail.Text = If(IsDBNull(DataGridView1.Rows(e.RowIndex).Cells("Email").Value), "", DataGridView1.Rows(e.RowIndex).Cells("Email").Value.ToString())
                cmbUserRole.Text = DataGridView1.Rows(e.RowIndex).Cells("Role").Value.ToString()

                ' FIXED: Handle Active field which is now "Yes"/"No" string
                Dim activeValue As Object = DataGridView1.Rows(e.RowIndex).Cells("Active").Value
                If Not IsDBNull(activeValue) Then
                    Dim activeStr As String = activeValue.ToString().ToLower()
                    chkIsActive.Checked = (activeStr = "yes" Or activeStr = "true" Or activeStr = "1")
                Else
                    chkIsActive.Checked = True
                End If

                txtUsername.Enabled = False
                txtPassword.Enabled = False
                txtConfirmPassword.Enabled = False
                txtPassword.Clear()
                txtConfirmPassword.Clear()

                btnAdd.Enabled = False
                btnUpdate.Enabled = True
                btnDelete.Enabled = True
                btnResetPassword.Enabled = True

                lblPasswordNote.Text = "To change password, use 'Reset Password' button"
                lblPasswordNote.ForeColor = Color.Blue
            Catch ex As Exception
                MessageBox.Show("Error selecting user: " & ex.Message & vbCrLf & vbCrLf & ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    '==========================
    ' ACTIVITY LOG
    '==========================
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
            ' Optional: handle logging errors
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' FORM CLOSING
    '==========================
    Private Sub frmUserManagement_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            ' Close any open connections
            If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                con.Close()
            End If

            ' Clear DataGridView binding
            If DataGridView1 IsNot Nothing Then
                DataGridView1.DataSource = Nothing
            End If

            ' Dispose DataTable
            If dt IsNot Nothing Then
                dt.Dispose()
                dt = Nothing
            End If

        Catch ex As Exception
            Debug.WriteLine("UserManagement FormClosing error: " & ex.Message)
        End Try
    End Sub

    Private Sub frmUserManagement_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            ' Dispose connection
            If con IsNot Nothing Then
                con.Dispose()
                con = Nothing
            End If

            GC.SuppressFinalize(Me)
        Catch
        End Try
    End Sub
End Class