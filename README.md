# Естественный Отбор: Симуляция Эволюции Клеток

![Пример симуляции](https://i.imgur.com/zCkyVOs.png)

## Описание

Этот проект представляет собой симуляцию естественного отбора, где клетки эволюционируют в борьбе за существование.

## Скачать

- [Windows](https://github.com/Semka2014/NeuCells/raw/master/builds/NeuCells_windows.zip)
- Linux (в настоящее время недоступно)
- macOS (в настоящее время недоступно)

- Также требуется скачать библиотеку [SDL2](https://github.com/libsdl-org/SDL/releases/).

## Руководство по пользовательскому интерфейсу

-	**Кнопка 1**: Отображает количество кислорода и переключает режим отображения кислорода.
-	**Кнопка 2**: Переключает режимы отображения хищников, энергии, видов и времени жизни.
-	**Кнопка 3**: Включает\выключает запись симуляции.
-	**Кнопка 4**: Перезапускает симуляцию с рандомным сидом и удаляет сохранённые кадры.
-	**Кнопка 5**: Отображает сид, при нажатии можно ввести нужный сид.
-	**Кнопка O**: Открывает сохранённую симуляцию.
-	**Кнопка S**: Сохраняет текущую симуляцию.
   
   Остальные два числа обозначают количество ходов и количество клеток соответственно.

## Запись симуляции

Кадры симуляции сохраняются в папкe `sequence` с названием следующего формата: `frame_000001.png`. Частоту сохранения кадров вы можете настроить.
Для создания видеофайла можно использовать [FFmpeg](https://github.com/FFmpeg/FFmpeg) или любую другую программу на ваш выбор.

## Изменение правил симуляции

Правила симуляции - это, можно сказать, её настройки. Чтобы облегчить их изменение, они зафиксированы в коде только частично. Остальные параметры хранятся в текстовом файле настроек. Вы можете менять параметры, добавлять комментарии, менять расположения строк но не можете менять названия переменных (если не отразите это в коде) или удалять их.

- При сборке:
Насткройки хранятся в файле `.setting`. При сборке проекта этот файл копируется в каталог программы с именем `settings`.
Вам нужно редактировать именно файл `.setting`. Если после запуска настройки не применяются сделайте любое изменение в програмном коде и сразу уберите его. Это нужно, чтобы спровацировать сборку.
- В собранной программе:
Откройте файл `settings` в любом текстовом редакторе и измените его. Настройки вступят в силу после перезапуска программы.
