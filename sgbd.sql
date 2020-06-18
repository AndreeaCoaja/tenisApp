use sgbd 
go

create table Jucator(
	ID int not null Primary Key,
	Nume varchar(50),
	Prenume varchar(50),
	Tara varchar(50),
	Puncte_Curente int
)

insert into Jucator values(1, 'Halep','Simona', 'Romania', 120), (2, 'Kerber', 'Angelique', 'Germania', 100), (3, 'Wozniacki', 'Caroline', 'Danemarca', 120)

create table Angajat(
	ID int not null Primary Key,
	ID_Jucator int not null,
	Nume varchar(50),
	Functie varchar(50),
	FOREIGN KEY (ID_Jucator) REFERENCES Jucator(ID)
)

insert into Angajat values(1, 1, 'Dahren','antrenor'), (2, 1, 'Dobos','fizioterapeut'), (3, 2, 'Kindlmann','antrenor'), (4, 3, 'Wozniacki','antrenor'), (5, 1, 'Pop', 'nutritionist'), (6,2, 'Muller', 'bucatar'), (7, 3, 'Stall', 'fizioterapeut')


create table Liceu(
	ID int not null Primary Key,
	Nume_Liceu varchar(50),
	Oras varchar(50)
)

insert into Liceu values(1, 'CN Eudoxiu Hurmuzachi','Radauti'), (2, 'CN Petru Rares','Suceava'), (3, 'CN Onisifor Ghibu','Cluj')

create table Profesor(
	ID int not null Primary Key,
	ID_Liceu int not null,
	Nume varchar(50),
	Prenume varchar(50),
	Materie varchar(50),
	FOREIGN KEY (ID_Liceu) REFERENCES Liceu(ID)
)

insert into Profesor values(1,1, 'Pop','Ion', 'matemtica'), (2,2, 'Bla','Maria', 'romana'), (3,3, 'Pop','Mircea', 'matemtica'), (4,1, 'Rotar','Viorica', 'romana'), (5,2, 'Curelar','Natalia', 'matemtica'), (6,3, 'Borg','Ana', 'romana'), (7, 1, 'Popovici', 'Angelica', 'germana')

