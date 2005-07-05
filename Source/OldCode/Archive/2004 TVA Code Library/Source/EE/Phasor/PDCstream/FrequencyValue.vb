'*******************************************************************************************************
'  FrequencyValue.vb - Frequency value
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Interop

Namespace EE.Phasor.PDCstream

    Public Class FrequencyValue

        Inherits FrequencyValueBase

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Double, ByVal dfdt As Double)

            MyBase.New(parent, frequencyDefinition, frequency, dfdt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)

            MyBase.New(parent, frequencyDefinition, unscaledFrequency, unscaledDfDt)

        End Sub

        Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent, frequencyDefinition, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal frequencyValue As IFrequencyValue)

            MyBase.New(frequencyValue)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

    End Class

End Namespace