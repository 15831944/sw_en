﻿using BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFD.Infrastructure
{
    public class CFrameCalculations
    {
        //-------------------------------------------------------------------------------------------------------------------------------
        FrameCalculationsAsyncStub stub = null;
        public delegate void FrameCalculationsAsyncStub(CModel frame, bool bCalculateLoadCasesOnly);
        private Object theLock;

        //-------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------
        public CFrameCalculations(Object lockObject)
        {
            theLock = lockObject;
        }

        public void FrameCalculations(CModel frame, bool bCalculateLoadCasesOnly)
        {
            bool bConsiderNodalDisplacement = false; // Pre ramy nepouzijeme absolutne hodnoty deformacii, tj. vratane deformacie (posunov) koncovych uzlov ramu
            bool bConsiderNodalDisplacementOnly = true; // Pre ramy pouzijeme len priehyby od posunov uzlov, nezohladnuju sa relativne lokalne deformacie samotneho pruta
            CModelToBFEMNetConverter.Convert(frame, bCalculateLoadCasesOnly, bConsiderNodalDisplacementOnly, bConsiderNodalDisplacement);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        public IAsyncResult BeginFrameCalculations(CModel frame, bool bCalculateLoadCasesOnly, AsyncCallback cb, object s)
        {
            stub = new FrameCalculationsAsyncStub(FrameCalculations);
            //using delegate for asynchronous implementation
            return stub.BeginInvoke(frame, bCalculateLoadCasesOnly, cb, null);
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        public void EndFrameCalculations(IAsyncResult call)
        {
            stub.EndInvoke(call);
        }
    }
}
