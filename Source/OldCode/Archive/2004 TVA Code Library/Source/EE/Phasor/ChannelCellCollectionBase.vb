'*******************************************************************************************************
'  ChannelCellCollectionBase.vb - Channel data cell collection base class
'  Copyright � 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of a collection of any kind of data cell.
    Public MustInherit Class ChannelCellCollectionBase

        Inherits ChannelCollectionBase

        Public Shadows Sub Add(ByVal value As IChannelCell)

            MyBase.Add(value)

        End Sub

        Default Public Shadows ReadOnly Property Item(ByVal index As Integer) As IChannelCell
            Get
                Return MyBase.Item(index)
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Int16
            Get
                ' Cells will be different lengths, so we must manually sum lengths
                Dim length As Integer

                For x As Integer = 0 To List.Count - 1
                    length += Item(x).BinaryLength
                Next

                Return length
            End Get
        End Property

    End Class

End Namespace
