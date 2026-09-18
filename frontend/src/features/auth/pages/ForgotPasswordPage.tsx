import { Link } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { useEsqueciSenha } from "@/features/auth/hooks/useEsqueciSenha";
import { esqueciSenhaSchema, type EsqueciSenhaFormValues } from "@/features/auth/lib/schemas";

export default function ForgotPasswordPage() {
  const esqueciSenha = useEsqueciSenha();

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<EsqueciSenhaFormValues>({ resolver: zodResolver(esqueciSenhaSchema) });

  function onSubmit(dados: EsqueciSenhaFormValues) {
    esqueciSenha.mutate(dados);
  }

  // Sucesso e erro mostram a MESMA mensagem de confirmação - o backend
  // nunca deve revelar se o e-mail existe ou não na base (mesma lógica de
  // resposta genérica usada no login, Etapa 9.8).
  if (esqueciSenha.isSuccess || esqueciSenha.isError) {
    return (
      <div className="space-y-4 text-center">
        <CheckCircle2 className="mx-auto h-10 w-10 text-primary" />
        <h2 className="text-lg font-semibold">Verifique seu e-mail</h2>
        <p className="text-sm text-muted-foreground">
          Se houver uma conta associada a este e-mail, enviamos um link para redefinir sua senha.
        </p>
        <Link to="/login" className="text-sm font-medium text-primary hover:underline">
          Voltar para o login
        </Link>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <h2 className="text-lg font-semibold">Recuperar senha</h2>
      <p className="text-sm text-muted-foreground">
        Informe o e-mail cadastrado e enviaremos um link para redefinir sua senha.
      </p>

      <div className="space-y-2">
        <Label htmlFor="email">E-mail</Label>
        <Input id="email" type="email" autoComplete="email" {...register("email")} />
        {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
      </div>

      <Button type="submit" className="w-full" disabled={esqueciSenha.isPending}>
        {esqueciSenha.isPending ? "Enviando..." : "Enviar link de recuperação"}
      </Button>

      <p className="text-center text-sm text-muted-foreground">
        <Link to="/login" className="font-medium text-primary hover:underline">
          Voltar para o login
        </Link>
      </p>
    </form>
  );
}