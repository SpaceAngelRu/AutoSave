# AutoSave
Расширение для Visual studio, добавляет копирование всех классов проекта в буфер или в папку игры для вставки в скрипт Space Engineers.
 - Старт экспорта, после сохранения файлов проекта или по нажатию на кнопку Tools -> Save for Space Engineers
 - Код скрипта можно держать в разных файлах, алгоритм учтет вхождение в класс Program и добавит их в итоговоый скрипт.
 - Есть возможность иключить файл из обработки, нужно добавить //Exclude from project, в любое место файла.
 - Можно исключать пустые строки, комментарии и region блоки
 - Для добавления комментариев при включенном исключении, нужно добавить "-". Пример: //- ваш комментарий
 - Если нужно загружать скрипт из workshop, добавьте путь к c:\users\<Имя пользователя>\appData\roaming\spaceEngineers\ingameScripts\local\
 - Tools -> Options -> SaveForSe настройки.
 - Можно отключить сохранение в буффер обмена (Tools -> Options -> SaveForSe -> Enable save to buffer)

# Installation
Загрузить, запустить
https://github.com/SpaceAngelRu/AutoSave/releases
