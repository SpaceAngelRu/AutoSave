# AutoSave
Extension of Visual Studio. Copies Space Engineers script code to buffer

Расширение для Visual studio, добавляет авто сохранение скрипта в текущем документе.
 - Во время сохранения (Если включена в настройках, Tools -> Options -> SaveForSe -> Enable save to buffer)
 - После нажатия кнопки сохранения (Tools -> Save for Space Engineers)

```
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SeScripts.Dron
{
    //Start script

    public class Sample
    {
        
    }

    public sealed class Program : MyGridProgram
    {
        
    }

    //End script
}
```

- //Start script, //End script - точки начала и окончания скрипта (все, что между ними копируется в буффер)
- Tools -> Options -> SaveForSe - можно изменить текст точек.
