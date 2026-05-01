export interface LoginData {
    email: string | null;
    phoneNumber: string | null;
    password: string;
    name?: string | null;
    codeReferrer?: string | null;
}