﻿using DSMBF_DRMF.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSMBF_DRMF.Persistence.Interfaces
{
    public interface IClaimFigureDao
    {
        long SaveClaimFigure(ClaimFigure cf);
    }
}
