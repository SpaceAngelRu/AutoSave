# AutoSave
Extension of Visual Studio. Copies Space Engineers script code to buffer

Расширение для Visual studio, добавляет копирование всех классов проекта в буфер для вставки в скрипт Space Engineers.
 - Учитывается вхождение класса в Program. Теперь можно иметь полноценные классы в разных файлах. Классы будут вверху скрипта.
 - Во время сохранения (Если включена в настройках, Tools -> Options -> SaveForSe -> Enable save to buffer)
 - После нажатия кнопки сохранения (Tools -> Save for Space Engineers)
 - Можно исключать пустые строки, комментарии и region блоки
 - Для добавления комментариев при включенном исключении, нужно добавить "-". Пример: //- ваш комментарий
 - Можно прописать путь к c:\users\<user name>\appData\roaming\spaceEngineers\ingameScripts\local\, если нужно загружать скрипт не через буфер.

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
