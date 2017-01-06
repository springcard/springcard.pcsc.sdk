Imports SpringCard.PCSC

Public Class Form1

    Dim reader As SCardReader
    Dim cardchannel As SCardChannel

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        refreshReaders()
    End Sub

    Private Sub refreshReaders()
        cbReaders.Items.Clear()
        Try
            Dim readers() As String = SCARD.Readers()

            If Not IsNothing(readers) Then
                For Each reader As String In readers
                    cbReaders.Items.Add(reader)
                Next

                If cbReaders.Items.Count >= 0 Then
                    cbReaders.SelectedIndex = 0
                End If

                For i = 0 To cbReaders.Items.Count - 1
                    If cbReaders.Items(i).ToString().Contains("SpringCard") Then
                        cbReaders.SelectedIndex = i
                    End If

                Next
            End If
        Catch ex As Exception
            MessageBox.Show("There was an error while searching for the list of readers")
        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        refreshReaders()
        lblStatus.Text = ""
        lblCardAtr.Text = ""
        lblCardUid.Text = ""
    End Sub

    Private Sub cbReaders_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbReaders.SelectedIndexChanged
        If Not IsNothing(reader) Then
            reader.StopMonitor()
        End If

        reader = New SCardReader(cbReaders.Items(cbReaders.SelectedIndex).ToString)
        reader.StartMonitor(New SCardReader.StatusChangeCallback(AddressOf ReaderStatusChanged))
    End Sub

    Private Delegate Sub ReaderStatusChangedInvoker(ByVal ReaderState As UInteger, ByVal CardAtr As CardBuffer)
    Private Sub ReaderStatusChanged(ByVal ReaderState As UInteger, ByVal CardAtr As CardBuffer)
        If InvokeRequired Then
            BeginInvoke(New ReaderStatusChangedInvoker(AddressOf ReaderStatusChanged), ReaderState, CardAtr)
            Return
        End If
        lblStatus.Text = SCARD.ReaderStatusToString(ReaderState)

        If IsNothing(CardAtr) Then
            lblCardAtr.Text = ""
            lblCardUid.Text = ""
        Else
            lblCardAtr.Text = CardAtr.AsString(" ")
            cardchannel = New SCardChannel(reader)
            Dim capdu As CAPDU = New CAPDU(&HFF, &HCA, &H0, &H0)
            If (Not cardchannel.Connect()) Then
                Return
            End If

            Dim rapdu As RAPDU = cardchannel.Transmit(capdu)
            If IsNothing(rapdu) Then
                Return
            End If

            If (Not rapdu.SW = &H9000) Then
                lblCardUid.Text = "'Get UID' APDU has failed"
                lblCardUid.ForeColor = Color.Red
            Else
                Dim rapdu_bytes() As Byte = rapdu.data.GetBytes()
                Dim uid As String = ""
                For i As Integer = 0 To (rapdu.data.GetBytes().Length - 1)
                    uid += String.Format("{0:x02}", rapdu.data.GetByte(i)).ToUpper() + " "
                Next
                lblCardUid.Text = uid
                lblCardUid.ForeColor = Color.Green
            End If
        End If

    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If Not IsNothing(cardchannel) Then
            cardchannel.Disconnect()
        End If

        If Not IsNothing(reader) Then
            reader.StopMonitor()
        End If
    End Sub
End Class
