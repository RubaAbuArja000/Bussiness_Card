export class Card {
  cardId: number;
  name: string;
  gender: number;
  dateOfBirth: string;
  email: string;
  phone: string;
  address: string;
  photo: string;

  constructor(
    cardId: number = 0,
    name: string = '',
    gender: number = 0,
    dateOfBirth: string = '',
    email: string = '',
    phone: string = '',
    address: string = '',
    photo: string = ''
  ) {
    this.cardId = cardId;
    this.name = name;
    this.gender = gender;
    this.dateOfBirth = dateOfBirth;
    this.email = email;
    this.phone = phone;
    this.address = address;
    this.photo = photo;
  }
}
