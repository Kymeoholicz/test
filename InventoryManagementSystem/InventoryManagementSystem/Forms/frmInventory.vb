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

        AddHandler Me.HandleDestroyed, Sub() GC.Collect()

        ' Populate ComboBoxes
        PopulateComboBoxes()

        LoadData()
        ClearFields()
    End Sub

    ' ===== Populate ComboBoxes =====
    Private Sub PopulateComboBoxes()
        ' Category ComboBox
        cmbCategory.Items.Clear()
        cmbCategory.Items.Add("Electronics")
        cmbCategory.Items.Add("Furniture")
        cmbCategory.Items.Add("Office Supplies")
        cmbCategory.Items.Add("Equipment")
        cmbCategory.Items.Add("Tools")
        cmbCategory.Items.Add("Hardware")
        cmbCategory.Items.Add("Software")
        cmbCategory.Items.Add("Accessories")
        cmbCategory.Items.Add("Other")

        ' Condition ComboBox
        cmbCondition.Items.Clear()
        cmbCondition.Items.Add("New")
        cmbCondition.Items.Add("Good")
        cmbCondition.Items.Add("Fair")
        cmbCondition.Items.Add("Poor")
        cmbCondition.Items.Add("Under Repair")
        cmbCondition.Items.Add("Retired")

        ' Location ComboBox
        cmbLocation.Items.Clear()
        cmbLocation.Items.Add("Warehouse A")
        cmbLocation.Items.Add("Warehouse B")
        cmbLocation.Items.Add("Office - Floor 1")
        cmbLocation.Items.Add("Office - Floor 2")
        cmbLocation.Items.Add("Office - Floor 3")
        cmbLocation.Items.Add("Storage Room")
        cmbLocation.Items.Add("Main Store")
        cmbLocation.Items.Add("Branch Store")
        cmbLocation.Items.Add("Under Maintenance")
    End Sub

    ' ===== Load Data =====
    Private Sub LoadData()
        If isClosing Then Return

        If dgvInventory Is Nothing OrElse dgvInventory.IsDisposed Then Return

        SyncLock dataLock
            Try
                If currentDataTable IsNot Nothing Then
                    Try
                        dgvInventory.DataSource = Nothing
                        currentDataTable.Dispose()
                        currentDataTable = Nothing
                    Catch
                    End Try
                End If

                dgvInventory.DataSource = Nothing
                dgvInventory.Rows.Clear()

                Using con As OleDbConnection = DatabaseConfig.GetConnection()
                    con.Open()

                    Using cmd As New OleDbCommand("SELECT ItemID, ItemName, Category, Quantity, [Condition], [Location], DateAdded FROM tblInventory ORDER BY ItemID DESC", con)
                        Using reader As OleDbDataReader = cmd.ExecuteReader()
                            If dgvInventory.Columns.Count = 0 Then
                                dgvInventory.Columns.Add("ItemID", "ID")
                                dgvInventory.Columns.Add("ItemName", "Item Name")
                                dgvInventory.Columns.Add("Category", "Category")
                                dgvInventory.Columns.Add("Quantity", "Quantity")
                                dgvInventory.Columns.Add("Condition", "Condition")
                                dgvInventory.Columns.Add("Location", "Location")
                                dgvInventory.Columns.Add("DateAdded", "Date Added")

                                dgvInventory.Columns(0).Width = 60
                                dgvInventory.Columns(1).Width = 150
                                dgvInventory.Columns(2).Width = 100
                                dgvInventory.Columns(3).Width = 80
                                dgvInventory.Columns(4).Width = 100
                                dgvInventory.Columns(5).Width = 150
                                dgvInventory.Columns(6).Width = 120
                            End If

                            While reader.Read() AndAlso Not isClosing
                                dgvInventory.Rows.Add(
                                    If(IsDBNull(reader(0)), 0, reader(0)),
                                    If(IsDBNull(reader(1)), "", reader(1).ToString()),
                                    If(IsDBNull(reader(2)), "", reader(2).ToString()),
                                    If(IsDBNull(reader(3)), 0, reader(3)),
                                    If(IsDBNull(reader(4)), "", reader(4).ToString()),
                                    If(IsDBNull(reader(5)), "", reader(5).ToString()),
                                    If(IsDBNull(reader(6)), DateTime.Now, reader(6))
                                )
                            End While
                        End Using
                    End Using
                End Using

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
        txtQuantity.Clear()
        txtSearch.Clear()

        ' Clear ComboBoxes
        cmbCategory.SelectedIndex = -1
        cmbCondition.SelectedIndex = -1
        cmbLocation.SelectedIndex = -1

        ' Hide textboxes, show comboboxes
        txtCategory.Visible = False
        txtCondition.Visible = False
        txtLocation.Visible = False
        cmbCategory.Visible = True
        cmbCondition.Visible = True
        cmbLocation.Visible = True

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
                    cmd.Parameters.Add("@Category", OleDbType.VarWChar, 50).Value = cmbCategory.Text
                    cmd.Parameters.Add("@Quantity", OleDbType.Integer).Value = CInt(txtQuantity.Text.Trim())
                    cmd.Parameters.Add("@Condition", OleDbType.VarWChar, 50).Value = cmbCondition.Text
                    cmd.Parameters.Add("@Location", OleDbType.VarWChar, 100).Value = cmbLocation.Text
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
                    cmd.Parameters.Add("@Category", OleDbType.VarWChar, 50).Value = cmbCategory.Text
                    cmd.Parameters.Add("@Quantity", OleDbType.Integer).Value = CInt(txtQuantity.Text.Trim())
                    cmd.Parameters.Add("@Condition", OleDbType.VarWChar, 50).Value = cmbCondition.Text
                    cmd.Parameters.Add("@Location", OleDbType.VarWChar, 100).Value = cmbLocation.Text
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
                dgvInventory.DataSource = Nothing
                dgvInventory.Rows.Clear()

                Using con As OleDbConnection = DatabaseConfig.GetConnection()
                    con.Open()

                    Using cmd As New OleDbCommand("SELECT ItemID, ItemName, Category, Quantity, [Condition], [Location], DateAdded FROM tblInventory WHERE ItemName LIKE ? OR Category LIKE ? OR [Location] LIKE ? ORDER BY ItemID DESC", con)
                        Dim likeTerm As String = "%" & searchTerm & "%"
                        cmd.Parameters.Add("@ItemName", OleDbType.VarWChar, 100).Value = likeTerm
                        cmd.Parameters.Add("@Category", OleDbType.VarWChar, 50).Value = likeTerm
                        cmd.Parameters.Add("@Location", OleDbType.VarWChar, 100).Value = likeTerm

                        Using reader As OleDbDataReader = cmd.ExecuteReader()
                            While reader.Read() AndAlso Not isClosing
                                dgvInventory.Rows.Add(
                                    If(IsDBNull(reader(0)), 0, reader(0)),
                                    If(IsDBNull(reader(1)), "", reader(1).ToString()),
                                    If(IsDBNull(reader(2)), "", reader(2).ToString()),
                                    If(IsDBNull(reader(3)), 0, reader(3)),
                                    If(IsDBNull(reader(4)), "", reader(4).ToString()),
                                    If(IsDBNull(reader(5)), "", reader(5).ToString()),
                                    If(IsDBNull(reader(6)), DateTime.Now, reader(6))
                                )
                            End While
                        End Using
                    End Using
                End Using

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
                Try
                    SyncLock dataLock
                        If dgvInventory IsNot Nothing AndAlso Not dgvInventory.IsDisposed Then
                            dgvInventory.DataSource = Nothing
                            dgvInventory.Rows.Clear()
                        End If

                        currentDataTable = Nothing
                    End SyncLock

                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                Catch ex As Exception
                    Debug.WriteLine($"Cleanup error in Back button: {ex.Message}")
                End Try

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
            If dgvInventory Is Nothing OrElse dgvInventory.IsDisposed Then Return

            Dim row As DataGridViewRow = dgvInventory.Rows(e.RowIndex)
            If row Is Nothing OrElse row.Cells.Count < 6 Then Return

            selectedItemID = If(row.Cells(0).Value IsNot DBNull.Value, CInt(row.Cells(0).Value), 0)
            txtItemName.Text = If(row.Cells(1).Value IsNot DBNull.Value, row.Cells(1).Value.ToString(), "")
            txtQuantity.Text = If(row.Cells(3).Value IsNot DBNull.Value, row.Cells(3).Value.ToString(), "")

            ' Set ComboBox values
            Dim category As String = If(row.Cells(2).Value IsNot DBNull.Value, row.Cells(2).Value.ToString(), "")
            Dim condition As String = If(row.Cells(4).Value IsNot DBNull.Value, row.Cells(4).Value.ToString(), "")
            Dim location As String = If(row.Cells(5).Value IsNot DBNull.Value, row.Cells(5).Value.ToString(), "")

            ' Try to select existing items in combo boxes
            Dim categoryIndex As Integer = cmbCategory.FindStringExact(category)
            If categoryIndex >= 0 Then
                cmbCategory.SelectedIndex = categoryIndex
            Else
                cmbCategory.Items.Add(category)
                cmbCategory.SelectedIndex = cmbCategory.Items.Count - 1
            End If

            Dim conditionIndex As Integer = cmbCondition.FindStringExact(condition)
            If conditionIndex >= 0 Then
                cmbCondition.SelectedIndex = conditionIndex
            Else
                cmbCondition.Items.Add(condition)
                cmbCondition.SelectedIndex = cmbCondition.Items.Count - 1
            End If

            Dim locationIndex As Integer = cmbLocation.FindStringExact(location)
            If locationIndex >= 0 Then
                cmbLocation.SelectedIndex = locationIndex
            Else
                cmbLocation.Items.Add(location)
                cmbLocation.SelectedIndex = cmbLocation.Items.Count - 1
            End If

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

        If cmbCategory.SelectedIndex = -1 Then
            MessageBox.Show("Select a category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbCategory.Focus()
            Return False
        End If

        Dim qty As Integer
        If Not Integer.TryParse(txtQuantity.Text.Trim(), qty) OrElse qty < 0 Then
            MessageBox.Show("Enter valid quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtQuantity.Focus()
            Return False
        End If

        If cmbCondition.SelectedIndex = -1 Then
            MessageBox.Show("Select a condition.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbCondition.Focus()
            Return False
        End If

        If cmbLocation.SelectedIndex = -1 Then
            MessageBox.Show("Select a location.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbLocation.Focus()
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

    ' ===== Form Closing =====
    Private Sub frmInventory_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            isClosing = True
            Thread.Sleep(50)

            Try
                RemoveHandler dgvInventory.CellClick, AddressOf dgvInventory_CellClick
            Catch
            End Try

            Try
                SyncLock dataLock
                    If dgvInventory IsNot Nothing AndAlso Not dgvInventory.IsDisposed Then
                        dgvInventory.DataSource = Nothing
                    End If
                    currentDataTable = Nothing
                End SyncLock
            Catch
            End Try

            Try
                GC.Collect()
                GC.WaitForPendingFinalizers()
            Catch
            End Try

        Catch ex As Exception
            Debug.WriteLine("FormClosing error: " & ex.Message)
        End Try
    End Sub

    ' ===== Form Closed =====
    Private Sub frmInventory_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            GC.SuppressFinalize(Me)
        Catch
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