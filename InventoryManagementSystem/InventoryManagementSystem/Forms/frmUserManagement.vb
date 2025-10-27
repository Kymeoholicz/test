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

        ' Window settings
        Me.Text = "User Management - Admin Only"
        Me.WindowState = FormWindowState.Normal
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1000, 660)
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

        ' Load role filter
        cmbRoleFilter.Items.Clear()
        cmbRoleFilter.Items.Add("All Roles")
        cmbRoleFilter.Items.Add("Admin")
        cmbRoleFilter.Items.Add("Manager")
        cmbRoleFilter.Items.Add("User")
        cmbRoleFilter.SelectedIndex = 0
    End Sub

    Private Sub LoadData(Optional filterRole As String = "")
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim sql As String = "SELECT UserID AS [ID], Username, FullName AS [Full Name], Email, UserRole AS [Role], IsActive AS [Active], Format(DateCreated, 'yyyy-MM-dd') AS [Created] FROM tblUsers"

            ' Apply role filter if specified
            If Not String.IsNullOrEmpty(filterRole) AndAlso filterRole <> "All Roles" Then
                sql &= " WHERE UserRole = ?"
            End If

            sql &= " ORDER BY UserID DESC"

            da = New OleDbDataAdapter(sql, con)
            If Not String.IsNullOrEmpty(filterRole) AndAlso filterRole <> "All Roles" Then
                da.SelectCommand.Parameters.AddWithValue("@1", filterRole)
            End If

            dt = New DataTable
            da.Fill(dt)
            DataGridView1.DataSource = dt

            ' Format DataGridView
            If DataGridView1.Columns.Count > 0 Then
                DataGridView1.Columns(0).Width = 50
                DataGridView1.Columns(1).Width = 120
                DataGridView1.Columns(2).Width = 180
                DataGridView1.Columns(3).Width = 200
                DataGridView1.Columns(4).Width = 80
                DataGridView1.Columns(5).Width = 70
                DataGridView1.Columns(6).Width = 100
            End If

            ' Update statistics
            lblTotalUsers.Text = "Total Users: " & dt.Rows.Count.ToString()
            Dim admins = dt.AsEnumerable().Count(Function(r) r.Field(Of String)("Role") = "Admin")
            Dim managers = dt.AsEnumerable().Count(Function(r) r.Field(Of String)("Role") = "Manager")
            Dim users = dt.AsEnumerable().Count(Function(r) r.Field(Of String)("Role") = "User")
            lblUserStats.Text = $"Admins: {admins} | Managers: {managers} | Users: {users}"
        Catch ex As Exception
            MessageBox.Show("Error loading data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
        DataGridView1.ClearSelection()
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
    ' VALIDATION
    '==========================
    Private Function ValidateInput(isUpdate As Boolean) As Boolean
        ' Username validation (only for new users)
        If Not isUpdate AndAlso String.IsNullOrWhiteSpace(txtUsername.Text) Then
            MessageBox.Show("Please enter a username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtUsername.Focus()
            Return False
        End If

        ' Full name validation
        If String.IsNullOrWhiteSpace(txtFullName.Text) Then
            MessageBox.Show("Please enter full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtFullName.Focus()
            Return False
        End If

        ' Email validation (basic)
        If Not String.IsNullOrWhiteSpace(txtEmail.Text) AndAlso Not txtEmail.Text.Contains("@") Then
            MessageBox.Show("Please enter a valid email address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtEmail.Focus()
            Return False
        End If

        ' Password validation (only for new users)
        If Not isUpdate Then
            If String.IsNullOrWhiteSpace(txtPassword.Text) Then
                MessageBox.Show("Please enter a password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtPassword.Focus()
                Return False
            End If

            If txtPassword.Text.Length < 6 Then
                MessageBox.Show("Password must be at least 6 characters long.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtPassword.Focus()
                Return False
            End If

            If txtPassword.Text <> txtConfirmPassword.Text Then
                MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtConfirmPassword.Focus()
                Return False
            End If
        End If

        Return True
    End Function

    '==========================
    ' ADD USER
    '==========================
    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If Not ValidateInput(False) Then Return

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            ' Check if username exists
            cmd = New OleDbCommand("SELECT COUNT(*) FROM tblUsers WHERE Username = ?", con)
            cmd.Parameters.AddWithValue("@1", txtUsername.Text.Trim())
            Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            If count > 0 Then
                MessageBox.Show("Username already exists. Please choose a different username.", "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtUsername.Focus()
                Return
            End If

            ' Insert new user
            cmd = New OleDbCommand("INSERT INTO tblUsers (Username, FullName, Email, [UserPassword], UserRole, IsActive, DateCreated, CreatedBy) VALUES (?,?,?,?,?,?,?,?)", con)
            cmd.Parameters.AddWithValue("@1", txtUsername.Text.Trim())
            cmd.Parameters.AddWithValue("@2", txtFullName.Text.Trim())
            cmd.Parameters.AddWithValue("@3", If(String.IsNullOrWhiteSpace(txtEmail.Text), "", txtEmail.Text.Trim()))
            cmd.Parameters.AddWithValue("@4", HashPassword(txtPassword.Text.Trim()))
            cmd.Parameters.AddWithValue("@5", cmbUserRole.Text)
            cmd.Parameters.AddWithValue("@6", chkIsActive.Checked)
            cmd.Parameters.AddWithValue("@7", Date.Now)
            cmd.Parameters.AddWithValue("@8", CurrentUser.UserID)
            cmd.ExecuteNonQuery()

            LogActivity("Add User", "Added user: " & txtUsername.Text)
            MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            LoadData()
            ClearFields()
        Catch ex As Exception
            MessageBox.Show("Error adding user: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' UPDATE USER
    '==========================
    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        If selectedUserID = 0 Then
            MessageBox.Show("Please select a user to update.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not ValidateInput(True) Then Return

        ' Prevent updating own account to inactive
        If selectedUserID = CurrentUser.UserID AndAlso Not chkIsActive.Checked Then
            MessageBox.Show("You cannot deactivate your own account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            cmd = New OleDbCommand("UPDATE tblUsers SET FullName=?, Email=?, UserRole=?, IsActive=? WHERE UserID=?", con)
            cmd.Parameters.AddWithValue("@1", txtFullName.Text.Trim())
            cmd.Parameters.AddWithValue("@2", If(String.IsNullOrWhiteSpace(txtEmail.Text), "", txtEmail.Text.Trim()))
            cmd.Parameters.AddWithValue("@3", cmbUserRole.Text)
            cmd.Parameters.AddWithValue("@4", chkIsActive.Checked)
            cmd.Parameters.AddWithValue("@5", selectedUserID)
            cmd.ExecuteNonQuery()

            LogActivity("Update User", "Updated user: " & txtUsername.Text & " (ID: " & selectedUserID & ")")
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
            MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Prevent deleting own account
        If selectedUserID = CurrentUser.UserID Then
            MessageBox.Show("You cannot delete your own account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim confirm = MessageBox.Show("Are you sure you want to delete user: " & txtUsername.Text & "?" & vbCrLf & vbCrLf & "This action cannot be undone!", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If confirm = DialogResult.No Then Return

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            cmd = New OleDbCommand("DELETE FROM tblUsers WHERE UserID=?", con)
            cmd.Parameters.AddWithValue("@1", selectedUserID)
            cmd.ExecuteNonQuery()

            LogActivity("Delete User", "Deleted user: " & txtUsername.Text & " (ID: " & selectedUserID & ")")
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
        If selectedUserID = 0 Then
            MessageBox.Show("Please select a user to reset password.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim resetForm As New frmResetPassword(selectedUserID, txtUsername.Text)
            If resetForm.ShowDialog() = DialogResult.OK Then
                LogActivity("Reset Password", "Reset password for user: " & txtUsername.Text & " (ID: " & selectedUserID & ")")
                MessageBox.Show("Password reset successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ClearFields()
            End If
        Catch ex As Exception
            MessageBox.Show("Error opening password reset form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '==========================
    ' SEARCH
    '==========================
    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Dim searchTerm As String = txtSearch.Text.Trim()

        If String.IsNullOrWhiteSpace(searchTerm) Then
            LoadData()
            Return
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim sql As String = "SELECT UserID AS [ID], Username, FullName AS [Full Name], Email, UserRole AS [Role], IsActive AS [Active], Format(DateCreated, 'yyyy-MM-dd') AS [Created] FROM tblUsers WHERE Username LIKE ? OR FullName LIKE ? OR Email LIKE ? ORDER BY UserID DESC"

            da = New OleDbDataAdapter(sql, con)
            Dim likeParam As String = "%" & searchTerm & "%"
            da.SelectCommand.Parameters.AddWithValue("@1", likeParam)
            da.SelectCommand.Parameters.AddWithValue("@2", likeParam)
            da.SelectCommand.Parameters.AddWithValue("@3", likeParam)

            dt = New DataTable
            da.Fill(dt)
            DataGridView1.DataSource = dt

            lblTotalUsers.Text = "Search Results: " & dt.Rows.Count.ToString()

            If dt.Rows.Count = 0 Then
                MessageBox.Show("No users found matching your search criteria.", "No Results", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Error searching: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' ROLE FILTER
    '==========================
    Private Sub cmbRoleFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbRoleFilter.SelectedIndexChanged
        If cmbRoleFilter.SelectedIndex = 0 Then
            LoadData() ' All Roles
        Else
            LoadData(cmbRoleFilter.Text) ' Specific role
        End If
    End Sub

    '==========================
    ' REFRESH
    '==========================
    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            txtSearch.Clear()
            If cmbRoleFilter.Items.Count > 0 Then cmbRoleFilter.SelectedIndex = 0
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
                Dim row As DataGridViewRow = DataGridView1.Rows(e.RowIndex)

                selectedUserID = Convert.ToInt32(row.Cells("ID").Value)
                txtUsername.Text = row.Cells("Username").Value.ToString()
                txtFullName.Text = row.Cells("Full Name").Value.ToString()
                txtEmail.Text = If(row.Cells("Email").Value IsNot DBNull.Value, row.Cells("Email").Value.ToString(), "")
                cmbUserRole.Text = row.Cells("Role").Value.ToString()
                chkIsActive.Checked = Convert.ToBoolean(row.Cells("Active").Value)

                ' Disable username and password fields when editing
                txtUsername.Enabled = False
                txtPassword.Enabled = False
                txtConfirmPassword.Enabled = False
                txtPassword.Clear()
                txtConfirmPassword.Clear()

                ' Enable/disable buttons
                btnAdd.Enabled = False
                btnUpdate.Enabled = True
                btnDelete.Enabled = True
                btnResetPassword.Enabled = True

                lblPasswordNote.Text = "Use 'Reset Password' button to change password"
                lblPasswordNote.ForeColor = Color.Blue
            Catch ex As Exception
                MessageBox.Show("Error selecting user: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            ' Silently fail - don't interrupt user operations for logging errors
            Debug.WriteLine("Error logging activity: " & ex.Message)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    '==========================
    ' ENTER KEY SUPPORT
    '==========================
    Private Sub txtSearch_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtSearch.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            e.Handled = True
            btnSearch_Click(Nothing, Nothing)
        End If
    End Sub
End Class