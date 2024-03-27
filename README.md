

<html>
<body lang="ru-RU" link="#000080" vlink="#800000" dir="ltr"><p style="line-height: 100%; page-break-inside: avoid; margin-bottom: 0.11cm; page-break-before: auto; page-break-after: avoid"><a name="_qjqhi2gz239r"></a>
<p>Для работы надо скачать оба проекта в одну папку: "Cloudfactory.BrokerEmulator" и "Cloudfactory.MessageBroker</p>
<font size="6" style="font-size: 26pt"><b>Тестовое задание
на бэк разработчика .NET</b></font></p>
<h1 style="page-break-before: auto; page-break-after: auto"><a name="_g4vtminfk5q6"></a>
Легенда</h1>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Представим себе систему, обрабатывающую
входящий http трафик, запрашивая бэкэнд
и отдающая его результат в ответе.
Общение с бэкэндом нужно спроектировать
через брокера сообщений - т.е. апи бэкэнда
не имеет синхронного апи типа
“request-reply”.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<h1 style="page-break-before: auto; page-break-after: auto"><a name="_iugd9euxeakc"></a>
Задача</h1>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Нужно реализовать две версии, реализующие
решение задачи.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
В “наивной” (primitive) реализации системы
все входящие запросы поступают в брокер
и ожидают ответа от него, который и
передают вызывающему.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
В продвинутой (advanced) реализации требуется
схлопывать идентичные запросы в один
запрос брокеру. Идентичность определяется
через функцию, извлекающую  из запроса
ключ. После процедуры схлопывания (т.е.
отсылки ответов вызывающим), следующая
пачка “таких же” запросов вновь
порождает запрос брокеру.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<h1 style="page-break-before: auto; page-break-after: auto"><a name="_kolzk8sn4l32"></a>
Реализация (замечания по имлементации)</h1>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
В качестве брокера использовать
директорию, где запрос (method, path) кладется
в файл  с именем “ключ запроса.req”</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Ответ брокера ожидается в файле “ключ
запроса.resp”,где первой строкой будет
http код, а остальное - тело ответа для
вызывающего. После вычитки ответа файлы
ответа и запроса должны удаляться с
диска сервисом.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Продумать как будет осуществляться
смена реализации на “настоящие”
брокеры, обладают возможностью уведомления
клиентов о сообщении в канале (например
- любой amqp брокер).</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Продумать различные варианты ошибок,
например: недоступность брокера (=нет
каталога в нашей реализации), таймауты
вызывающих (сценарий - два вызывающих
с одним запросом с разницей во времени
1 минута, ответ через 1.5минуты от первого
запроса, а таймаут вызывающих - 1 минута),
падение сервиса и его рестарт (что делать
с очередью?), ошибка в формате ответа,
невозможность удаления ответа и/или
запроса.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Расчет ключа для сохранения файла
производить по формуле md5(http method + http
path). Не путать этот ключ с ключем,
используемым для схлопывания запросов
- он остается на усмотрение разработчика.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Ответы могут быть достаточно сильно
разнесены по времени с ответом, так, что
какой-то из вызывающих, ожидающих ответа,
может принудительно со своей стороны
разорвать соединение не дождавшись
ответа.</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
Настройки брокера в конфиг файле
(директория хранилища).</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<h1 style="page-break-before: auto; page-break-after: auto"><a name="_6iua31z13q6b"></a>
Как будет тестироваться, на что будет
обращаться внимание</h1>
<ul>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	Будет создаваться нагрузка в 10-20 тыс
	запросов, пачками по идентичности
	ключа, также в фоне будут создаваться
	различные ответы на пачки запросов и
	анализироваться результаты.</p></li>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	Логи читаются с stdout</p></li>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	Автоматически снимаем метрики по кол-ву
	потоков (системных) и по деградации по
	ним и памяти исходя из повышения нагрузки</p></li>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	Использование примитивов синхронизации</p></li>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	Общее исполнение задачи в коде, применение
	ООП, возможность расширения/замены
	“малой кровью” функционала (смена
	брокера, функций ключей, хранилища
	настроек и тп)</p></li>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	(не критично) Наличие юнит тестов</p></li>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	(не критично) наличие нагрузочных тестов
	и бенчмарков</p></li>
	<li><p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
	наличие мусора в коде</p></li>
</ul>
<p style="margin-left: 1.27cm; margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
<p style="margin-bottom: 0cm; page-break-before: auto; page-break-after: auto">
<br/>

</p>
</body>
</html>
