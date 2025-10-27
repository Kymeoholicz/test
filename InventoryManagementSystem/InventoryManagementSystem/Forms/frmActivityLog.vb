Imports System.Data.OleDb
Imports System.IO
Imports System.Text

Public Class frmActivityLog
    Dim con As OleDbConnection
    Dim da As OleDbDataAdapter
    Dim dt As DataTable
    Dim cmd As OleDbCommand
    Private isClosing As Boolean = False

    Private Sub frmActivityLog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        con = DatabaseConfig.GetConnection()
        LoadFilterTypes()
        LoadData()
        ClearFilters()
        Me.Text = "Activity Log Management"
        Me.WindowState = FormWindowState.Normal
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1024, 600)
    End Sub

    Private Sub LoadFilterTypes()
        If isClosing Then Return

        cmbFilterType.Items.Clear()
        cmbFilterType.Items.Add("All")
        cmbFilterType.Items.Add("User")
        cmbFilterType.Items.Add("Activity Type")
        cmbFilterType.Items.Add("Description")
        cmbFilterType.SelectedIndex = 0
    End Sub

    Private Sub LoadData(Optional sql As String = "")
        If isClosing Then Return

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()

            If String.IsNullOrWhiteSpace(sql) Then
                da = New OleDbDataAdapter("SELECT LogID, UserID, ActivityType, Description, Format(ActivityDate,'yyyy-MM-dd HH:nn:ss') AS [Date] FROM tblActivityLog ORDER BY ActivityDate DESC", con)
            Else
                da = New OleDbDataAdapter(sql, con)
            End If

            ' Dispose old DataTable first
            If dt IsNot Nothing Then
                Try
                    dt.Dispose()
                Catch
                End Try
            End If

            dt = New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt

            lblTotalRecords.Text = "Total Records: " & dt.Rows.Count.ToString()
            lblActivitySummary.Text = $"Summary: Showing {dt.Rows.Count} Records"

        Catch ex As Exception
            If Not isClosing Then
                MessageBox.Show("Error loading data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    Private Sub ClearFilters()
        If isClosing Then Return

        txtSearch.Clear()
        dtpFrom.Value = Date.Now.AddMonths(-1)
        dtpTo.Value = Date.Now
        If cmbFilterType.Items.Count > 0 Then cmbFilterType.SelectedIndex = 0
    End Sub

    ' Filter by search term
    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        If isClosing Then Return

        Dim searchTerm As String = "%" & txtSearch.Text.Trim() & "%"
        Dim filterColumn As String = ""

        Select Case cmbFilterType.SelectedItem.ToString()
            Case "User"
                filterColumn = "UserID"
            Case "Activity Type"
                filterColumn = "ActivityType"
            Case "Description"
                filterColumn = "Description"
            Case Else
                filterColumn = "UserID OR ActivityType OR Description"
        End Select

        Dim sql As String
        If filterColumn.Contains("OR") Then
            sql = "SELECT LogID, UserID, ActivityType, Description, Format(ActivityDate,'yyyy-MM-dd HH:nn:ss') AS [Date] FROM tblActivityLog " &
                  "WHERE UserID LIKE ? OR ActivityType LIKE ? OR Description LIKE ? ORDER BY ActivityDate DESC"
        Else
            sql = $"SELECT LogID, UserID, ActivityType, Description, Format(ActivityDate,'yyyy-MM-dd HH:nn:ss') AS [Date] FROM tblActivityLog WHERE {filterColumn} LIKE ? ORDER BY ActivityDate DESC"
        End If

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            da = New OleDbDataAdapter(sql, con)
            If filterColumn.Contains("OR") Then
                da.SelectCommand.Parameters.AddWithValue("@1", searchTerm)
                da.SelectCommand.Parameters.AddWithValue("@2", searchTerm)
                da.SelectCommand.Parameters.AddWithValue("@3", searchTerm)
            Else
                da.SelectCommand.Parameters.AddWithValue("@1", searchTerm)
            End If

            ' Dispose old DataTable
            If dt IsNot Nothing Then
                Try
                    dt.Dispose()
                Catch
                End Try
            End If

            dt = New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt
            lblTotalRecords.Text = "Total Records: " & dt.Rows.Count.ToString()
            lblActivitySummary.Text = $"Summary: Showing {dt.Rows.Count} Records"
        Catch ex As Exception
            If Not isClosing Then
                MessageBox.Show("Error searching: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    ' Filter by date range
    Private Sub btnDateFilter_Click(sender As Object, e As EventArgs) Handles btnDateFilter.Click
        If isClosing Then Return

        Dim fromDate As Date = dtpFrom.Value.Date
        Dim toDate As Date = dtpTo.Value.Date.AddDays(1).AddSeconds(-1)
        Dim sql As String = "SELECT LogID, UserID, ActivityType, Description, Format(ActivityDate,'yyyy-MM-dd HH:nn:ss') AS [Date] " &
                            "FROM tblActivityLog WHERE ActivityDate >= ? AND ActivityDate <= ? ORDER BY ActivityDate DESC"

        Try
            If con.State = ConnectionState.Open Then con.Close()
            con.Open()
            da = New OleDbDataAdapter(sql, con)
            da.SelectCommand.Parameters.AddWithValue("@1", fromDate)
            da.SelectCommand.Parameters.AddWithValue("@2", toDate)

            ' Dispose old DataTable
            If dt IsNot Nothing Then
                Try
                    dt.Dispose()
                Catch
                End Try
            End If

            dt = New DataTable()
            da.Fill(dt)
            DataGridView1.DataSource = dt
            lblTotalRecords.Text = "Total Records: " & dt.Rows.Count.ToString()
            lblActivitySummary.Text = $"Summary: Showing {dt.Rows.Count} Records"
        Catch ex As Exception
            If Not isClosing Then
                MessageBox.Show("Error filtering by date: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If con.State = ConnectionState.Open Then con.Close()
        End Try
    End Sub

    ' Refresh button
    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        If isClosing Then Return
        LoadData()
        ClearFilters()
    End Sub

    ' Clear old logs (90+ days)
    Private Sub btnClearOldLogs_Click(sender As Object, e As EventArgs) Handles btnClearOldLogs.Click
        If isClosing Then Return

        If MessageBox.Show("Are you sure you want to delete logs older than 90 days?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            Try
                If con.State = ConnectionState.Open Then con.Close()
                con.Open()
                cmd = New OleDbCommand("DELETE FROM tblActivityLog WHERE ActivityDate < ?", con)
                cmd.Parameters.AddWithValue("@1", Date.Now.AddDays(-90))
                Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                MessageBox.Show($"{rowsAffected} old log(s) deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                LoadData()
            Catch ex As Exception
                If Not isClosing Then
                    MessageBox.Show("Error clearing old logs: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            Finally
                If con.State = ConnectionState.Open Then con.Close()
            End Try
        End If
    End Sub

    ' Export to CSV
    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If isClosing Then Return

        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            MessageBox.Show("No data to export.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using sfd As New SaveFileDialog()
            sfd.Filter = "CSV files (*.csv)|*.csv"
            sfd.FileName = $"ActivityLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            If sfd.ShowDialog() = DialogResult.OK Then
                Try
                    Using sw As New StreamWriter(sfd.FileName, False, Encoding.UTF8)
                        ' Write header
                        Dim columnNames = dt.Columns.Cast(Of DataColumn).Select(Function(c) c.ColumnName)
                        sw.WriteLine(String.Join(",", columnNames))

                        ' Write rows
                        For Each row As DataRow In dt.Rows
                            Dim fields = row.ItemArray.Select(Function(f) """" & f.ToString().Replace("""", """""") & """")
                            sw.WriteLine(String.Join(",", fields))
                        Next
                    End Using
                    MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show("Error exporting CSV: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    ' === Form Closing Cleanup ===
    Private Sub frmActivityLog_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            isClosing = True

            ' Remove event handlers
            Try
                RemoveHandler btnSearch.Click, AddressOf btnSearch_Click
                RemoveHandler btnDateFilter.Click, AddressOf btnDateFilter_Click
                RemoveHandler btnRefresh.Click, AddressOf btnRefresh_Click
                RemoveHandler btnClearOldLogs.Click, AddressOf btnClearOldLogs_Click
                RemoveHandler btnExport.Click, AddressOf btnExport_Click
            Catch
            End Try

            ' Close connection
            Try
                If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                    con.Close()
                End If
            Catch
            End Try

            ' Clear DataGridView binding
            Try
                If DataGridView1 IsNot Nothing Then
                    DataGridView1.DataSource = Nothing
                End If
            Catch
            End Try

        Catch ex As Exception
            Debug.WriteLine("ActivityLog FormClosing error: " & ex.Message)
        End Try
    End Sub

    Private Sub frmActivityLog_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            ' Dispose DataTable
            If dt IsNot Nothing Then
                dt.Dispose()
                dt = Nothing
            End If

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