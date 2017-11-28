export class User {
  public name: string;
  public sub: string;

  public static fromPojo(pojo: any) {
    pojo = pojo || {};

    const user = new User();
    user.name = pojo.name;
    user.sub = pojo.sub;

    return user;
  }
}
