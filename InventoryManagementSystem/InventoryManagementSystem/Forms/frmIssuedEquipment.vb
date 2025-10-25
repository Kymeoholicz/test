Imports System.Data.OleDb

Public Class frmIssuedEquipment
    ' ===== Class-level variables =====
    Private con As OleDbConnection
    Private cmd As OleDbCommand
    Private da As OleDbDataAdapter
    Private dt As DataTable
    Private selectedIssueID As Integer = 0

    ' ===== Form Load =====
    Private Sub frmIssuedEquipment_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Issued Equipment Management"
        Me.WindowState = FormWindowState.Maximized
        con = DatabaseConfig.GetConnection()

        ' === MANUAL EVENT WIRING ===
        AddHandler btnIssue.Click, AddressOf btnIssue_Click
        AddHandler btnReturn.Click, AddressOf btnReturn_Click
        AddHandler btnClear.Click, AddressOf btnClear_Click
        AddHandler btnRefresh.Click, AddressOf btnRefresh_Click
        AddHandler btnSearch.Click, AddressOf btnSearch_Click
        AddHandler btnViewAll.Click, AddressOf btnViewAll_Click
        AddHandler btnViewOverdue.Click, AddressOf btnViewOverdue_Click
        AddHandler cmbItemID.SelectedIndexChanged, AddressOf cmbItemID_SelectedIndexChanged
        AddHandler DataGridView1.CellClick, AddressOf DataGridView1_CellClick

        LoadIssuedData()
        LoadItemsComboBox()
        ClearFields()

        dtpDateIssued.Value = Date.Now
        dtpReturnDate.Value = Date.Now.AddDays(7)
    End Sub

    ' ===== Load Items for ComboBox =====
    Private Sub LoadItemsComboBox()
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim query As String = "SELECT ItemID, (ItemName & ' - ' & Category) AS DisplayName, Quantity FROM tblInventory WHERE Quantity > 0 ORDER BY ItemName"
            da = New OleDbDataAdapter(query, con)
            dt = New DataTable()
            da.Fill(dt)

            cmbItemID.DataSource = dt
            cmbItemID.DisplayMember = "DisplayName"
            cmbItemID.ValueMember = "ItemID"

            If dt.Rows.Count > 0 Then cmbItemID.SelectedIndex = 0 Else MessageBox.Show("No available stock!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        Catch ex As Exception
            MessageBox.Show("Error loading items: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    ' ===== Load Issued Equipment Data =====
    Private Sub LoadIssuedData()
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim query As String = "SELECT ie.IssueID AS [ID], i.ItemName AS [Item Name], i.Category, ie.IssuedTo AS [Issued To], " &
                                  "Format(ie.DateIssued,'yyyy-MM-dd') AS [Date Issued], Format(ie.ReturnDate,'yyyy-MM-dd') AS [Expected Return], ie.Remarks " &
                                  "FROM tblIssuedEquipment ie INNER JOIN tblInventory i ON ie.ItemID=i.ItemID ORDER BY ie.IssueID DESC"
            da = New OleDbDataAdapter(query, con)
            dt = New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt

            FormatDataGrid()
            UpdateStats()

        Catch ex As Exception
            MessageBox.Show("Error loading issued data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    ' ===== Format DataGridView =====
    Private Sub FormatDataGrid()
        With DataGridView1
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .MultiSelect = False
            .ReadOnly = True
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .RowHeadersVisible = False
            .AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray
            .DefaultCellStyle.SelectionBackColor = Color.DarkBlue
            .DefaultCellStyle.SelectionForeColor = Color.White

            If .Columns.Count > 0 Then
                .Columns(0).Width = 50
                .Columns(1).Width = 150
                .Columns(2).Width = 100
                .Columns(3).Width = 150
                .Columns(4).Width = 100
                .Columns(5).Width = 100
                .Columns(6).Width = 200
            End If
        End With
    End Sub

    ' ===== Update Stats =====
    Private Sub UpdateStats()
        Dim overdueCount As Integer = 0
        For Each row As DataGridViewRow In DataGridView1.Rows
            Dim returnDateStr As String = If(row.Cells("Expected Return").Value IsNot Nothing, row.Cells("Expected Return").Value.ToString(), "")
            Dim returnDate As Date
            If Date.TryParse(returnDateStr, returnDate) Then
                If returnDate < Date.Now Then overdueCount += 1
            End If
        Next
        lblTotalIssued.Text = "Total Issued: " & DataGridView1.Rows.Count.ToString()
        lblOverdue.Text = "Overdue: " & overdueCount.ToString()
        lblOverdue.ForeColor = If(overdueCount > 0, Color.Red, Color.Green)
    End Sub

    ' ===== Clear Form Fields =====
    Private Sub ClearFields()
        If cmbItemID.Items.Count > 0 Then cmbItemID.SelectedIndex = 0
        txtIssuedTo.Clear()
        txtRemarks.Clear()
        dtpDateIssued.Value = Date.Now
        dtpReturnDate.Value = Date.Now.AddDays(7)
        selectedIssueID = 0
        btnIssue.Enabled = True
        btnReturn.Enabled = False
        lblAvailableQty.Text = "Available: 0"
        UpdateAvailableQuantity()
    End Sub

    ' ===== Get Available Quantity =====
    Private Function GetAvailableQuantity(itemID As Integer) As Integer
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            cmd = New OleDbCommand("SELECT Quantity FROM tblInventory WHERE ItemID=?", con)
            cmd.Parameters.Add("@ItemID", OleDbType.Integer).Value = itemID
            Dim result = cmd.ExecuteScalar()
            If result IsNot Nothing Then Return CInt(result)
        Catch ex As Exception
            MessageBox.Show("Error getting quantity: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
        Return 0
    End Function

    ' ===== Update Available Quantity Label =====
    Private Sub UpdateAvailableQuantity()
        Try
            If cmbItemID.SelectedIndex >= 0 AndAlso cmbItemID.SelectedValue IsNot Nothing Then
                Dim itemID As Integer = CInt(cmbItemID.SelectedValue)
                Dim qty As Integer = GetAvailableQuantity(itemID)
                lblAvailableQty.Text = "Available: " & qty.ToString()
                lblAvailableQty.ForeColor = If(qty <= 0, Color.Red, If(qty <= 5, Color.Orange, Color.Green))
            End If
        Catch
        End Try
    End Sub

    Private Sub cmbItemID_SelectedIndexChanged(sender As Object, e As EventArgs)
        UpdateAvailableQuantity()
    End Sub

    ' ===== Issue Item ===== (NO HANDLES CLAUSE)
    Private Sub btnIssue_Click(sender As Object, e As EventArgs)
        If cmbItemID.SelectedIndex = -1 Or String.IsNullOrWhiteSpace(txtIssuedTo.Text) Then
            MessageBox.Show("Select item and enter recipient.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim itemID As Integer = CInt(cmbItemID.SelectedValue)
        Dim issuedTo As String = txtIssuedTo.Text.Trim()
        Dim remarks As String = txtRemarks.Text.Trim()
        Dim availableQty As Integer = GetAvailableQuantity(itemID)
        If availableQty <= 0 Then
            MessageBox.Show("No available stock.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            Using cmd = New OleDbCommand("INSERT INTO tblIssuedEquipment (ItemID, IssuedTo, DateIssued, ReturnDate, Remarks) VALUES (?,?,?,?,?)", con)
                cmd.Parameters.Add("@ItemID", OleDbType.Integer).Value = itemID
                cmd.Parameters.Add("@IssuedTo", OleDbType.VarWChar, 100).Value = issuedTo
                cmd.Parameters.Add("@DateIssued", OleDbType.Date).Value = dtpDateIssued.Value
                cmd.Parameters.Add("@ReturnDate", OleDbType.Date).Value = dtpReturnDate.Value
                cmd.Parameters.Add("@Remarks", OleDbType.VarWChar, 255).Value = remarks
                cmd.ExecuteNonQuery()
            End Using

            Using cmd = New OleDbCommand("UPDATE tblInventory SET Quantity = Quantity - 1 WHERE ItemID=?", con)
                cmd.Parameters.Add("@ItemID", OleDbType.Integer).Value = itemID
                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Item issued successfully to " & issuedTo, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadItemsComboBox()
            LoadIssuedData()
            ClearFields()
        Catch ex As Exception
            MessageBox.Show("Error issuing item: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    ' ===== Return Item ===== (NO HANDLES CLAUSE)
    Private Sub btnReturn_Click(sender As Object, e As EventArgs)
        If selectedIssueID <= 0 Then
            MessageBox.Show("Select an issued item to return.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If MessageBox.Show("Mark this item as returned?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim transaction = con.BeginTransaction()
            Try
                cmd = New OleDbCommand("SELECT ItemID FROM tblIssuedEquipment WHERE IssueID=?", con, transaction)
                cmd.Parameters.Add("@IssueID", OleDbType.Integer).Value = selectedIssueID
                Dim itemID As Integer = CInt(cmd.ExecuteScalar())

                cmd = New OleDbCommand("DELETE FROM tblIssuedEquipment WHERE IssueID=?", con, transaction)
                cmd.Parameters.Add("@IssueID", OleDbType.Integer).Value = selectedIssueID
                cmd.ExecuteNonQuery()

                cmd = New OleDbCommand("UPDATE tblInventory SET Quantity = Quantity + 1 WHERE ItemID=?", con, transaction)
                cmd.Parameters.Add("@ItemID", OleDbType.Integer).Value = itemID
                cmd.ExecuteNonQuery()

                transaction.Commit()
                MessageBox.Show("Item returned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                LoadItemsComboBox()
                LoadIssuedData()
                ClearFields()

            Catch ex As Exception
                transaction.Rollback()
                Throw
            End Try
        Catch ex As Exception
            MessageBox.Show("Error returning item: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    ' ===== DataGridView Selection ===== (NO HANDLES CLAUSE)
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        Dim row As DataGridViewRow = DataGridView1.Rows(e.RowIndex)
        selectedIssueID = If(row.Cells(0).Value IsNot Nothing, CInt(row.Cells(0).Value), 0)
        lblSelectedItem.Text = "Selected: " & If(row.Cells(1).Value IsNot Nothing, row.Cells(1).Value.ToString(), "") &
                               " (Issued to: " & If(row.Cells(3).Value IsNot Nothing, row.Cells(3).Value.ToString(), "") & ")"
        btnIssue.Enabled = False
        btnReturn.Enabled = True
    End Sub

    ' ===== Clear Selection ===== (NO HANDLES CLAUSE)
    Private Sub btnClear_Click(sender As Object, e As EventArgs)
        ClearFields()
        DataGridView1.ClearSelection()
        lblSelectedItem.Text = "Selected: None"
    End Sub

    ' ===== Refresh Data ===== (NO HANDLES CLAUSE)
    Private Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadItemsComboBox()
        LoadIssuedData()
        ClearFields()
    End Sub

    ' ===== Search ===== (NO HANDLES CLAUSE)
    Private Sub btnSearch_Click(sender As Object, e As EventArgs)
        Dim searchTerm As String = txtSearch.Text.Trim()
        If String.IsNullOrWhiteSpace(searchTerm) Then
            LoadIssuedData()
            Return
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim query As String = "SELECT ie.IssueID AS [ID], i.ItemName AS [Item Name], i.Category, ie.IssuedTo AS [Issued To], " &
                                  "Format(ie.DateIssued,'yyyy-MM-dd') AS [Date Issued], Format(ie.ReturnDate,'yyyy-MM-dd') AS [Expected Return], ie.Remarks " &
                                  "FROM tblIssuedEquipment ie INNER JOIN tblInventory i ON ie.ItemID=i.ItemID " &
                                  "WHERE i.ItemName LIKE ? OR ie.IssuedTo LIKE ? OR i.Category LIKE ? " &
                                  "ORDER BY ie.IssueID DESC"

            da = New OleDbDataAdapter(query, con)
            Dim likeTerm As String = "%" & searchTerm & "%"
            da.SelectCommand.Parameters.AddWithValue("@1", likeTerm)
            da.SelectCommand.Parameters.AddWithValue("@2", likeTerm)
            da.SelectCommand.Parameters.AddWithValue("@3", likeTerm)

            dt = New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt

            FormatDataGrid()
            UpdateStats()

        Catch ex As Exception
            MessageBox.Show("Error searching: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    ' ===== View All ===== (NO HANDLES CLAUSE)
    Private Sub btnViewAll_Click(sender As Object, e As EventArgs)
        txtSearch.Clear()
        LoadIssuedData()
    End Sub

    ' ===== View Overdue ===== (NO HANDLES CLAUSE)
    Private Sub btnViewOverdue_Click(sender As Object, e As EventArgs)
        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            Dim query As String = "SELECT ie.IssueID AS [ID], i.ItemName AS [Item Name], i.Category, ie.IssuedTo AS [Issued To], " &
                                  "Format(ie.DateIssued,'yyyy-MM-dd') AS [Date Issued], Format(ie.ReturnDate,'yyyy-MM-dd') AS [Expected Return], ie.Remarks " &
                                  "FROM tblIssuedEquipment ie INNER JOIN tblInventory i ON ie.ItemID=i.ItemID " &
                                  "WHERE ie.ReturnDate < ? " &
                                  "ORDER BY ie.ReturnDate ASC"

            da = New OleDbDataAdapter(query, con)
            da.SelectCommand.Parameters.AddWithValue("@1", Date.Now)

            dt = New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt

            FormatDataGrid()
            UpdateStats()

            If dt.Rows.Count = 0 Then
                MessageBox.Show("No overdue items found!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading overdue items: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub
End Class