using Microsoft.AspNetCore.Components;
using Spix.xNetwork.PingHelper;

namespace Spix.AppFront.SharedUtility;

public partial class PingResultView
{
    [Parameter] public PingResult? Result { get; set; }
}