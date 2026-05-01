# processes-CHP-1.1-dz

Практичне завдання з системного програмування: робота з процесами у Windows.

## Що реалізовано

- Віконний інтерфейс Windows Forms.
- Перегляд списку процесів.
- Налаштування інтервалу оновлення списку.
- Вибір процесу зі списку.
- Відображення детальної інформації:
  - ідентифікатор процесу;
  - час старту;
  - загальний процесорний час;
  - кількість потоків;
  - кількість запущених копій процесу з такою самою назвою.
- Завершення вибраного процесу.
- Запуск програм:
  - Notepad;
  - Calculator;
  - Paint;
  - власна програма за шляхом або назвою.

## Файли

- `README.md` - опис і запуск.
- `ProcessManagerApp.cs` - весь код програми.

## Компіляція

У PowerShell з папки репозиторію:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:ProcessManagerApp.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll ProcessManagerApp.cs
```

Якщо у вас 32-bit Windows або немає `Framework64`, використайте:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /target:winexe /out:ProcessManagerApp.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll ProcessManagerApp.cs
```

## Запуск

Після компіляції:

```powershell
.\ProcessManagerApp.exe
```

Деякі системні процеси можуть не показувати час старту або процесорний час без прав адміністратора. Це нормальна поведінка Windows.
