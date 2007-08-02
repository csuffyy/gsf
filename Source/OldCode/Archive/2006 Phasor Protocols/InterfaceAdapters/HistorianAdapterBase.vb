'*******************************************************************************************************
'  HistorianAdapterBase.vb - Historian adpater base class
'  Copyright � 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/01/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports TVA.Measurements

Public MustInherit Class HistorianAdapterBase

    Inherits AdapterBase
    Implements IHistorianAdapter

    ''' <summary>This event will be raised if there is an exception encountered while attempting to archive measurements</summary>
    ''' <remarks>Connection cycle to historian will be restarted when an exception is encountered</remarks>
    Public Event ArchivalException(ByVal source As String, ByVal ex As Exception) Implements IHistorianAdapter.ArchivalException

    ''' <summary>This event gets raised every second allowing consumer to track total number of unarchived measurements</summary>
    ''' <remarks>If queue size reaches an unhealthy threshold, evasive action should be considered</remarks>
    Public Event UnarchivedMeasurements(ByVal total As Integer) Implements IHistorianAdapter.UnarchivedMeasurements

    Private m_measurementQueue As List(Of IMeasurement)
    Private m_dataProcessingThread As Thread
    Private m_processedMeasurements As Long
    Private m_updateProcessCountLock As Object
    Private WithEvents m_connectionTimer As Timers.Timer
    Private WithEvents m_monitorTimer As Timers.Timer

    Private Const ProcessedMeasurementInterval As Integer = 100000

    Public Sub New()

        m_measurementQueue = New List(Of IMeasurement)
        m_updateProcessCountLock = New Object

        m_connectionTimer = New Timers.Timer

        With m_connectionTimer
            .AutoReset = False
            .Interval = 2000
            .Enabled = False
        End With

        m_monitorTimer = New Timers.Timer

        ' We monitor total number of unarchived measurements every second - this is a useful statistic to monitor, if
        ' total number of unarchived measurements gets very large, measurement archival could be falling behind
        With m_monitorTimer
            .Interval = 1000
            .AutoReset = True
            .Enabled = False
        End With

    End Sub

    Public MustOverride Sub Initialize(ByVal connectionString As String) Implements IHistorianAdapter.Initialize

    Public Sub Connect() Implements IHistorianAdapter.Connect

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        ' Start the connection cycle
        m_connectionTimer.Enabled = True

    End Sub

    Protected MustOverride Sub AttemptConnection()

    Private Sub m_connectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_connectionTimer.Elapsed

        Try
            UpdateStatus("Starting connection attempt to " & Name & "...")

            ' Attempt connection to historian (consumer to call historian API connect function)
            ' This must happen before aborting data processing thread to keep disconnect from failing
            ' due to thread abort in case of reconnect caused by a processing exception
            AttemptConnection()

            ' Start data processing thread
            m_dataProcessingThread = New Thread(AddressOf ProcessMeasurements)
            m_dataProcessingThread.Start()

            UpdateStatus("Connection to " & Name & " established.")
        Catch ex As Exception
            UpdateStatus("WARNING: Connection to " & Name & " failed: " & ex.Message)
            Connect()
        End Try

    End Sub

    Public Sub Disconnect() Implements IHistorianAdapter.Disconnect

        Try
            Dim performedDisconnect As Boolean

            ' Attempt disconnection from historian (consumer to call historian API disconnect function)
            AttemptDisconnection()

            ' Stop data processing thread
            If m_dataProcessingThread IsNot Nothing Then
                m_dataProcessingThread.Abort()
                performedDisconnect = True
            End If
            m_dataProcessingThread = Nothing

            If performedDisconnect Then UpdateStatus("Disconnected from " & Name)
        Catch ex As Exception
            UpdateStatus("Exception occured during disconnect from " & Name & ": " & ex.Message)
        End Try

    End Sub

    Public Overrides Sub Dispose()

        Disconnect()

    End Sub

    Protected MustOverride Sub AttemptDisconnection()

    Public Overridable Sub QueueMeasurementForArchival(ByVal measurement As IMeasurement) Implements IHistorianAdapter.QueueMeasurementForArchival

        SyncLock m_measurementQueue
            m_measurementQueue.Add(measurement)
        End SyncLock

        ' We throw status message updates on the thread pool so we don't slow sorting operations
        ThreadPool.QueueUserWorkItem(AddressOf IncrementProcessedMeasurements, 1)

    End Sub

    Public Overridable Sub QueueMeasurementsForArchival(ByVal measurements As ICollection(Of IMeasurement)) Implements IHistorianAdapter.QueueMeasurementsForArchival

        SyncLock m_measurementQueue
            m_measurementQueue.AddRange(measurements)
        End SyncLock

        ' We throw status message updates on the thread pool so we don't slow sorting operations
        ThreadPool.QueueUserWorkItem(AddressOf IncrementProcessedMeasurements, measurements.Count)

    End Sub

    'Private Sub IncrementProcessedMeasurements()

    '    Interlocked.Increment(m_processedMeasurements)
    '    If m_processedMeasurements Mod ProcessedMeasurementInterval = 0 Then UpdateStatus(m_processedMeasurements.ToString("#,##0") & " measurements have been queued for archival so far...")

    'End Sub

    Private Sub IncrementProcessedMeasurements(ByVal state As Object)

        ' Since multiple threads may be calling this status update at the same time, we synchronize access to this code to prevent
        ' multiple messages being displayed at nearly the same time
        SyncLock m_updateProcessCountLock
            ' Check to see if total number of added points will exceed process interval used to show periodic
            ' messages of how many points have been archived so far...
            Dim totalAdded As Integer = CInt(state)
            Dim showMessage As Boolean = (m_processedMeasurements + totalAdded >= (m_processedMeasurements \ ProcessedMeasurementInterval + 1) * ProcessedMeasurementInterval)

            m_processedMeasurements += totalAdded

            If showMessage Then UpdateStatus(m_processedMeasurements.ToString("#,##0") & " measurements have been queued for archival so far...")
        End SyncLock

    End Sub

    ''' <summary>This function returns a range of measurements from the internal measurement queue, then removes the values</summary>
    ''' <remarks>
    ''' This method is typically only used to curtail size of measurement queue if it's getting too large.  If more points are
    ''' requested than there are points available - all points in the queue will be returned.
    ''' </remarks>
    Public Function GetMeasurements(ByVal total As Integer) As IMeasurement() Implements IHistorianAdapter.GetMeasurements

        Dim measurements As IMeasurement()

        SyncLock m_measurementQueue
            If total > m_measurementQueue.Count Then total = m_measurementQueue.Count
            measurements = m_measurementQueue.GetRange(0, total).ToArray()
            m_measurementQueue.RemoveRange(0, total)
        End SyncLock

        Return measurements

    End Function

    Protected Property ConnectionAttemptInterval() As Integer
        Get
            Return m_connectionTimer.Interval
        End Get
        Set(ByVal value As Integer)
            m_connectionTimer.Interval = value
        End Set
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("      Historian connection: ")
                If m_dataProcessingThread Is Nothing Then
                    .Append("Inactive - not connected")
                Else
                    .Append("Active")
                End If
                .Append(Environment.NewLine)
                .Append("  Queued measurement count: ")
                SyncLock m_measurementQueue
                    .Append(m_measurementQueue.Count)
                End SyncLock
                .Append(Environment.NewLine)
                .Append("     Archived measurements: ")
                .Append(m_processedMeasurements)
                .Append(Environment.NewLine)
                Return .ToString()
            End With
        End Get
    End Property

    Protected Sub RaiseArchivalException(ByVal ex As Exception)

        RaiseEvent ArchivalException(Name, ex)

    End Sub

    ''' <summary>User implemented function used to send queued measurements to archive</summary>
    Protected MustOverride Sub ArchiveMeasurements(ByVal measurements As IMeasurement())

    Private Sub ProcessMeasurements()

        Dim measurements As IMeasurement()

        Do While True
            Try
                ' Grab a copy of all queued measurements and send to historian adapter when
                ' we can get a lock - this way the "queuing" of data will get lock priority
                ' and we'll keep lock time to a minimum
                measurements = Nothing

                If Monitor.TryEnter(m_measurementQueue) Then
                    Try
                        If m_measurementQueue.Count > 0 Then
                            measurements = m_measurementQueue.ToArray()
                            m_measurementQueue.Clear()
                        End If
                    Finally
                        Monitor.Exit(m_measurementQueue)
                    End Try
                End If

                If measurements IsNot Nothing Then ArchiveMeasurements(measurements)

                ' We sleep thread between polls to reduce CPU loading...
                Thread.Sleep(1)
            Catch ex As ThreadAbortException
                ' If we received an abort exception, we'll egress gracefully
                Exit Do
            Catch ex As ObjectDisposedException
                ' This will be a normal exception...
                Exit Do
            Catch ex As Exception
                ' If an exception is thrown by the archiver, we'll report the event and restart the connection cycle
                RaiseArchivalException(ex)
                Connect()
                Exit Do
            End Try
        Loop

    End Sub

    ' All we do here is expose the total number of unarchived measurements in the queue
    Private Sub m_monitorTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_monitorTimer.Elapsed

        RaiseEvent UnarchivedMeasurements(m_measurementQueue.Count)

    End Sub

End Class
