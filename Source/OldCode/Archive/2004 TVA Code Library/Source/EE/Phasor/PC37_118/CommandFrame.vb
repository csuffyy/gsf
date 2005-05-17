'***********************************************************************
'  CommandFrame.vb - PC37_118 Command Frame
'  Copyright � 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor.PC37_118

    ' This class represents a command frame that can be sent to a PMU to elicit a desired response.  ABB PMU's won't begin
    ' a data broadcast until a command has been sent to "turn on" the real-time stream.
    Public Class CommandFrame

        Public Const FrameLength As Integer = 16

        Private m_timetag As NtpTimeTag
        Private m_pmuIDCode As Int64
        Private m_command As PMUCommand

        ' Use this contructor to send a command to a PMU
        Public Sub New(ByVal pmuIDCode As Int64, ByVal command As PMUCommand)

            m_pmuIDCode = pmuIDCode
            m_command = command
            m_timetag = New NtpTimeTag(DateTime.Now)

        End Sub

        ' Use this constuctor to receive a command (i.e., your code is acting as a PMU)
        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            If binaryImage Is Nothing Then
                Throw New ArgumentNullException("BinaryImage was null - could not create command frame")
            ElseIf binaryImage.Length - startIndex < FrameLength Then
                Throw New ArgumentException("BinaryImage size from startIndex is too small - could not create command frame")
            Else
                m_timetag = New NtpTimeTag(Convert.ToDouble(EndianOrder.ReverseToInt32(binaryImage, startIndex)))
                m_pmuIDCode = EndianOrder.ReverseToInt64(binaryImage, startIndex + 4)
                m_command = EndianOrder.ReverseToInt16(binaryImage, startIndex + 12)

                ' Validate buffer check sum
                If EndianOrder.ReverseToInt16(binaryImage, startIndex + FrameLength - 2) <> CRC16(-1, binaryImage, startIndex, FrameLength - 2) Then _
                    Throw New ArgumentException("Invalid buffer image detected - CRC16 of command frame did not match")
            End If

        End Sub

        Public Property TimeTag() As NtpTimeTag
            Get
                Return m_timetag
            End Get
            Set(ByVal Value As NtpTimeTag)
                m_timetag = Value
            End Set
        End Property

        Public Property PMUIDCode() As Int64
            Get
                Return m_pmuIDCode
            End Get
            Set(ByVal Value As Int64)
                m_pmuIDCode = Value
            End Set
        End Property

        Public Property Command() As PMUCommand
            Get
                Return m_command
            End Get
            Set(ByVal Value As PMUCommand)
                m_command = Value
            End Set
        End Property

        Public ReadOnly Property BinaryImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), FrameLength)

                EndianOrder.SwapCopyBytes(Convert.ToUInt32(m_timetag.Value), buffer, 0)
                EndianOrder.SwapCopyBytes(m_pmuIDCode, buffer, 4)
                EndianOrder.SwapCopyBytes(Convert.ToInt16(m_command), buffer, 12)
                EndianOrder.SwapCopyBytes(CRC16(-1, buffer, 0, 14), buffer, 14)

                Return buffer
            End Get
        End Property

    End Class

End Namespace