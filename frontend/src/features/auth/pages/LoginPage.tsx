import { Link } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { AlertCircle } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { useLogin } from "@/features/auth/hooks/useLogin";
import { loginSchema, type LoginFormValues } from "@/features/auth/lib/schemas";
import { normalizarErroApi } from "@/features/auth/lib/api-error";

export default function LoginPage() {
  const login = useLogin();

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  function onSubmit(dados: LoginFormValues) {
    login.mutate(dados);
  }

  const erro = login.error ? normalizarErroApi(login.error) : null;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <h2 className="text-lg font-semibold">Entrar</h2>

      {erro && (
        <div className="flex items-center gap-2 rounded-md bg-destructive/10 p-3 text-sm text-destructive">
          <AlertCircle className="h-4 w-4 shrink-0" />
          {erro.mensagemGeral}
        </div>
      )}

      <div className="space-y-2">
        <Label htmlFor="email">E-mail</Label>
        <Input id="email" type="email" autoComplete="email" {...register("email")} />
        {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="senha">Senha</Label>
          <Link to="/esqueci-senha" className="text-xs text-muted-foreground hover:underline">
            Esqueceu a senha?
          </Link>
        </div>
        <Input id="senha" type="password" autoComplete="current-password" {...register("senha")} />
        {errors.senha && <p className="text-sm text-destructive">{errors.senha.message}</p>}
      </div>

      <Button type="submit" className="w-full" disabled={login.isPending}>
        {login.isPending ? "Entrando..." : "Entrar"}
      </Button>

      <p className="text-center text-sm text-muted-foreground">
        Não tem uma conta?{" "}
        <Link to="/registrar" className="font-medium text-primary hover:underline">
          Cadastre sua empresa
        </Link>
      </p>
    </form>
  );
}