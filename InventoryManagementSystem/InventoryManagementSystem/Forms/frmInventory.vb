Imports System.Data.OleDb
Imports System.Threading

Public Class frmInventory
    ' ===== Class-level variables =====
    Private selectedItemID As Integer = 0
    Private isClosing As Boolean = False
    Private currentDataTable As DataTable = Nothing
    Private ReadOnly dataLock As New Object()

    ' ===== Form Load =====
    Private Sub frmInventory_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Inventory Management"
        Me.WindowState = FormWindowState.Maximized

        LoadData()
        ClearFields()
    End Sub

    ' ===== Load Data =====
    Private Sub LoadData()
        If isClosing Then Return

        ' Critical: Check if controls still exist
        If dgvInventory Is Nothing OrElse dgvInventory.IsDisposed Then Return

        SyncLock dataLock
            Try
                ' Simple unbind without dispose
                dgvInventory.DataSource = Nothing

                ' Create NEW DataTable
                Dim newDt As New DataTable()

                Using con As OleDbConnection = DatabaseConfig.GetConnection()
                    con.Open()

                    Using cmd As New OleDbCommand("SELECT ItemID, ItemName, Category, Quantity, [Condition], [Location], DateAdded FROM tblInventory ORDER BY ItemID DESC", con)
                        Using adapter As New OleDbDataAdapter(cmd)
                            adapter.Fill(newDt)
                        End Using
                    End Using
                End Using

                ' Dispose OLD DataTable after creating new one
                If currentDataTable IsNot Nothing Then
                    Try
                        currentDataTable.Dispose()
                    Catch
                    End Try
                End If

                ' Assign new DataTable
                currentDataTable = newDt
                dgvInventory.DataSource = currentDataTable

                ' Format columns after binding
                If dgvInventory.Columns.Count > 0 Then
                    dgvInventory.Columns(0).HeaderText = "ID"
                    dgvInventory.Columns(1).HeaderText = "Item Name"
                    dgvInventory.Columns(2).HeaderText = "Category"
                    dgvInventory.Columns(3).HeaderText = "Quantity"
                    dgvInventory.Columns(4).HeaderText = "Condition"
                    dgvInventory.Columns(5).HeaderText = "Location"
                    dgvInventory.Columns(6).HeaderText = "Date Added"

                    dgvInventory.Columns(0).Width = 60
                    dgvInventory.Columns(1).Width = 150
                    dgvInventory.Columns(2).Width = 100
                    dgvInventory.Columns(3).Width = 80
                    dgvInventory.Columns(4).Width = 100
                    dgvInventory.Columns(5).Width = 150
                    dgvInventory.Columns(6).Width = 120
                End If

            Catch ex As Exception
                If Not isClosing Then
                    MessageBox.Show("Error loading data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End Try
        End SyncLock
    End Sub

    ' ===== Clear Fields =====
    Private Sub ClearFields()
        txtItemName.Clear()
        txtCategory.Clear()
        txtQuantity.Clear()
        txtCondition.Clear()
        txtLocation.Clear()
        txtSearch.Clear()
        selectedItemID = 0

        btnAdd.Enabled = True
        btnUpdate.Enabled = False
        btnDelete.Enabled = False
    End Sub

    ' ===== Add Item =====
    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If Not ValidateInput() Then Return

        Try
            Using con As OleDbConnection = DatabaseConfig.GetConnection()
                con.Open()

                Using cmd As New OleDbCommand("INSERT INTO tblInventory (ItemName, Category, Quantity, [Condition], [Location], DateAdded) VALUES (?,?,?,?,?,?)", con)
                    cmd.Parameters.Add("@ItemName", OleDbType.VarWChar, 100).Value = txtItemName.Text.Trim()
                    cmd.Parameters.Add("@Category", OleDbType.VarWChar, 50).Value = txtCategory.Text.Trim()
                    cmd.Parameters.Add("@Quantity", OleDbType.Integer).Value = CInt(txtQuantity.Text.Trim())
                    cmd.Parameters.Add("@Condition", OleDbType.VarWChar, 50).Value = txtCondition.Text.Trim()
                    cmd.Parameters.Add("@Location", OleDbType.VarWChar, 100).Value = txtLocation.Text.Trim()
                    cmd.Parameters.Add("@DateAdded", OleDbType.Date).Value = Date.Now
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LogActivity("Add Inventory", $"Added item: {txtItemName.Text.Trim()}")
            LoadData()
            ClearFields()
        Catch ex As Exception
            MessageBox.Show("Error adding item: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ===== Update Item =====
    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        If selectedItemID <= 0 OrElse Not ValidateInput() Then Return

        Try
            Using con As OleDbConnection = DatabaseConfig.GetConnection()
                con.Open()

                Using cmd As New OleDbCommand("UPDATE tblInventory SET ItemName=?, Category=?, Quantity=?, [Condition]=?, [Location]=? WHERE ItemID=?", con)
                    cmd.Parameters.Add("@ItemName", OleDbType.VarWChar, 100).Value = txtItemName.Text.Trim()
                    cmd.Parameters.Add("@Category", OleDbType.VarWChar, 50).Value = txtCategory.Text.Trim()
                    cmd.Parameters.Add("@Quantity", OleDbType.Integer).Value = CInt(txtQuantity.Text.Trim())
                    cmd.Parameters.Add("@Condition", OleDbType.VarWChar, 50).Value = txtCondition.Text.Trim()
                    cmd.Parameters.Add("@Location", OleDbType.VarWChar, 100).Value = txtLocation.Text.Trim()
                    cmd.Parameters.Add("@ItemID", OleDbType.Integer).Value = selectedItemID
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Item updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LogActivity("Update Inventory", $"Updated item ID: {selectedItemID}")
            LoadData()
            ClearFields()
        Catch ex As Exception
            MessageBox.Show("Error updating item: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ===== Delete Item =====
    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If selectedItemID <= 0 Then Return
        If MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

        Try
            Using con As OleDbConnection = DatabaseConfig.GetConnection()
                con.Open()

                Using cmd As New OleDbCommand("DELETE FROM tblInventory WHERE ItemID=?", con)
                    cmd.Parameters.Add("@ItemID", OleDbType.Integer).Value = selectedItemID
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LogActivity("Delete Inventory", $"Deleted item ID: {selectedItemID}")

            If Not isClosing Then
                LoadData()
                ClearFields()
            End If
        Catch ex As Exception
            MessageBox.Show("Error deleting item: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ===== Search Item =====
    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Dim searchTerm As String = txtSearch.Text.Trim()
        If searchTerm = "" Then
            LoadData()
            Return
        End If

        If isClosing Then Return

        SyncLock dataLock
            Try
                ' Unbind
                dgvInventory.DataSource = Nothing

                ' Create new DataTable for search results
                Dim searchDt As New DataTable()

                Using con As OleDbConnection = DatabaseConfig.GetConnection()
                    con.Open()

                    Using cmd As New OleDbCommand("SELECT ItemID, ItemName, Category, Quantity, [Condition], [Location], DateAdded FROM tblInventory WHERE ItemName LIKE ? OR Category LIKE ? OR [Location] LIKE ? ORDER BY ItemID DESC", con)
                        Dim likeTerm As String = "%" & searchTerm & "%"
                        cmd.Parameters.Add("@ItemName", OleDbType.VarWChar, 100).Value = likeTerm
                        cmd.Parameters.Add("@Category", OleDbType.VarWChar, 50).Value = likeTerm
                        cmd.Parameters.Add("@Location", OleDbType.VarWChar, 100).Value = likeTerm

                        Using adapter As New OleDbDataAdapter(cmd)
                            adapter.Fill(searchDt)
                        End Using
                    End Using
                End Using

                ' Dispose old DataTable
                If currentDataTable IsNot Nothing Then
                    Try
                        currentDataTable.Dispose()
                    Catch
                    End Try
                End If

                ' Assign new search results
                currentDataTable = searchDt
                dgvInventory.DataSource = currentDataTable

            Catch ex As Exception
                If Not isClosing Then
                    MessageBox.Show("Error searching: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End Try
        End SyncLock
    End Sub

    ' ===== Back Button =====
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Try
            If MessageBox.Show("Return to Main Menu?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                ' Simply close this dialog form - returns to main menu automatically
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End If
        Catch ex As Exception
            MessageBox.Show($"Error returning to main menu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ===== DataGridView Cell Click =====
    Private Sub dgvInventory_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvInventory.CellClick
        If isClosing Then Return
        If e.RowIndex < 0 Then Return

        Try
            ' Prevent access during form closing
            If dgvInventory Is Nothing OrElse dgvInventory.IsDisposed Then Return

            Dim row As DataGridViewRow = dgvInventory.Rows(e.RowIndex)

            ' Validate row exists and has cells
            If row Is Nothing OrElse row.Cells.Count < 6 Then Return

            selectedItemID = If(row.Cells(0).Value IsNot DBNull.Value, CInt(row.Cells(0).Value), 0)
            txtItemName.Text = If(row.Cells(1).Value IsNot DBNull.Value, row.Cells(1).Value.ToString(), "")
            txtCategory.Text = If(row.Cells(2).Value IsNot DBNull.Value, row.Cells(2).Value.ToString(), "")
            txtQuantity.Text = If(row.Cells(3).Value IsNot DBNull.Value, row.Cells(3).Value.ToString(), "")
            txtCondition.Text = If(row.Cells(4).Value IsNot DBNull.Value, row.Cells(4).Value.ToString(), "")
            txtLocation.Text = If(row.Cells(5).Value IsNot DBNull.Value, row.Cells(5).Value.ToString(), "")

            btnAdd.Enabled = False
            btnUpdate.Enabled = True
            btnDelete.Enabled = True
        Catch ex As Exception
            If Not isClosing Then
                MessageBox.Show("Error selecting row: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    ' ===== Validate Input =====
    Private Function ValidateInput() As Boolean
        If String.IsNullOrWhiteSpace(txtItemName.Text) Then
            MessageBox.Show("Enter item name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtItemName.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtCategory.Text) Then
            MessageBox.Show("Enter category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtCategory.Focus()
            Return False
        End If

        Dim qty As Integer
        If Not Integer.TryParse(txtQuantity.Text.Trim(), qty) OrElse qty < 0 Then
            MessageBox.Show("Enter valid quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtQuantity.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtCondition.Text) Then
            MessageBox.Show("Enter condition.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtCondition.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtLocation.Text) Then
            MessageBox.Show("Enter location.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtLocation.Focus()
            Return False
        End If

        Return True
    End Function

    ' ===== Log Activity =====
    Private Sub LogActivity(activityType As String, description As String)
        If isClosing Then Return

        Try
            Using con As OleDbConnection = DatabaseConfig.GetConnection()
                con.Open()

                Using logCmd As New OleDbCommand("INSERT INTO tblActivityLog (UserID, ActivityType, Description, ActivityDate) VALUES (?,?,?,?)", con)
                    logCmd.Parameters.Add("@UserID", OleDbType.Integer).Value = CurrentUser.UserID
                    logCmd.Parameters.Add("@ActivityType", OleDbType.VarWChar, 50).Value = activityType
                    logCmd.Parameters.Add("@Description", OleDbType.VarWChar, 255).Value = description
                    logCmd.Parameters.Add("@ActivityDate", OleDbType.Date).Value = Date.Now
                    logCmd.ExecuteNonQuery()
                End Using
            End Using
        Catch
            ' Ignore logging errors silently
        End Try
    End Sub

    ' ===== Form Closing - Clean up COM objects =====
    Private Sub frmInventory_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            ' Set flag FIRST to stop all operations
            isClosing = True

            ' Remove event handlers to prevent any callbacks during cleanup
            Try
                RemoveHandler dgvInventory.CellClick, AddressOf dgvInventory_CellClick
            Catch
            End Try

            ' DO NOTHING ELSE - Let .NET handle all cleanup automatically
            ' Any manual cleanup can cause COM access violations

        Catch ex As Exception
            ' Swallow ALL exceptions during form closing
            Debug.WriteLine("FormClosing error: " & ex.Message)
        End Try
    End Sub

    ' ===== Form Closed - Final cleanup =====
    Private Sub frmInventory_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' Minimal cleanup - just suppress finalization
        Try
            GC.SuppressFinalize(Me)
        Catch
            ' Ignore any errors
        End Try
    End Sub

    ' ===== Buttons =====
    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        ClearFields()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadData()
        ClearFields()
    End Sub
End Class